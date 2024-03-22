/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Extensions;
using Corsinvest.ProxmoxVE.Admin.Core.Extensions;
using Corsinvest.ProxmoxVE.Admin.NodeProtect.Repository;
using Corsinvest.ProxmoxVE.NodeProtect.Api;

namespace Corsinvest.ProxmoxVE.Admin.NodeProtect;

internal class Helper
{
    public static string GetDirectoryWork(string clusterName) => Path.Combine(new Module().PathData, clusterName);
    public static string GetDirectoryWorkJobId(string clusterName, string jobId) => Path.Combine(GetDirectoryWork(clusterName), jobId);
    public static string GetPath(string clusterName, string jobId, string fileName) => Path.Combine(GetDirectoryWorkJobId(clusterName, jobId), fileName);

    public static void UploadToNode(IServiceScope scope, NodeProtectJobHistory item)
    {
        var clusterOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ClusterOptions>>().Value;
        var loggerFactory = scope.GetLoggerFactory();

        var logger = loggerFactory.CreateLogger(typeof(Helper));

        using (logger.LogTimeOperation(LogLevel.Information, true, "Node protect upload"))
        {
            Application.UploadToNode(clusterOptions.SshHostsAndPortHA,
                                     clusterOptions.SshCredential.Username,
                                     clusterOptions.SshCredential.Password,
                                     GetPath(item.ClusterName, item.JobId, item.FileName),
                                     loggerFactory);
        }
    }

    public static async Task Protect(IServiceScope scope, string clusterName)
    {
        var loggerFactory = scope.GetLoggerFactory();
        var logger = loggerFactory.CreateLogger(typeof(Helper));

        using (logger.LogTimeOperation(LogLevel.Information, true, "Node protect backup cluster '{clusterName}'", clusterName))
        {
            var directoryWork = GetDirectoryWork(clusterName);
            Directory.CreateDirectory(directoryWork);

            var log = "";
            using var swLog = new StringWriterEvent();
            swLog.WritedData += (sender, e) => log = swLog.ToString();

            var status = false;
            var start = DateTime.Now;
            var moduleClusterOptions = scope.GetModuleClusterOptions<Options, ModuleClusterOptions>(clusterName);

            try
            {
                var pveClientService = scope.GetPveClientService();
                var clusterOptions = pveClientService.GetClusterOptions(clusterName)!;

                Application.Backup(clusterOptions.SshHostsAndPortHA,
                                   clusterOptions.SshCredential.Username,
                                   clusterOptions.SshCredential.Password,
                                   moduleClusterOptions.PathsToBackup.SplitNewLine(),
                                   directoryWork,
                                   moduleClusterOptions.Keep,
                                   swLog,
                                   loggerFactory);
                status = true;
            }
            catch (Exception ex) { logger.LogError(ex, ex.Message); }

            var end = DateTime.Now;

            //last directory
            var path = Directory.GetDirectories(directoryWork)
                                .OrderByDescending(a => a)
                                .FirstOrDefault()!;

            var jobId = path.Split(Path.DirectorySeparatorChar)[^1];

            var jobHistoryRepo = scope.GetRepository<NodeProtectJobHistory>();
            foreach (var item in Directory.GetFiles(path))
            {
                var fileName = Path.GetFileName(item);
                var history = new NodeProtectJobHistory()
                {
                    JobId = jobId,
                    ClusterName = clusterName,
                    Start = start,
                    End = end,
                    Status = status,
                    FileName = fileName,
                    IpAddress = fileName.Split("-")[0],
                    Log = log,
                    Size = File.Exists(item)
                            ? new FileInfo(item).Length
                            : 0
                };
                await jobHistoryRepo.AddAsync(history);
                await jobHistoryRepo.SaveChangesAsync();
            }

            //keep history
            var histories = (await jobHistoryRepo.ListAsync(new NodeProtectJobHistorySpec(clusterName)))
                                .DistinctBy(a => a.Start)
                                .Skip(moduleClusterOptions.Keep)
                                .ToList();

            if (histories.Count != 0)
            {
                await jobHistoryRepo.DeleteRangeAsync(histories);
                await jobHistoryRepo.SaveChangesAsync();
            }
        }
    }
}