/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Diagnostics;
using System.Text;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Helpers;

public static class BackupHelper
{
    public record NodeBackupFile(string Node, string Logs, string FileName);

    public static async Task<IEnumerable<NodeBackupFile>> CreateAsync(ClusterClient clusterClient,
                                                                      IEnumerable<string> paths,
                                                                      string directoryWork,
                                                                      ILogger logger)
    {
        var totalSw = Stopwatch.StartNew();
        var files = new List<NodeBackupFile>();
        var date = DateTime.Now;
        var fileNameTarGz = $"/tmp/{date:yyyy-MM-dd-HH-mm-ss}.tar.gz";

        var nodes = (await clusterClient.CachedData.GetResourcesAsync(false))
                        .Where(a => a.ResourceType == ClusterResourceType.Node && a.IsOnline);

        foreach (var item in nodes)
        {
            var logs = new StringBuilder();

            // Create tar.gz
            var sw = Stopwatch.StartNew();
            logs.AppendLine($"[{item.Node}] Create file tar.gz: {fileNameTarGz}");
            var cmd = $"tar --one-file-system -cvzPf {fileNameTarGz} {paths.JoinAsString(" ")}";
            var result = await clusterClient.SshExecuteAsync(item.Node, true, [cmd]);
            var ret = result[0];
            sw.Stop();

            logger.LogDebug("Create tar.gz: {cmd}", cmd);
            logger.LogDebug("Result: {exitCode}, Time: {time}ms", ret.ExitCode, sw.ElapsedMilliseconds);

            logs.Append(ret.StdOut);
            logs.AppendLine($"[{item.Node}] Created tar.gz in {sw.Elapsed.TotalSeconds:F2} sec");

            // Download file
            sw.Restart();
            var fileToSave = Path.Combine(directoryWork, $"{item.Node}-config.tar.gz");

            logs.AppendLine($"[{item.Node}] Download file tar.gz: {fileNameTarGz} to {fileToSave}");
            using var sftpClient = await clusterClient.GetSftpClientAsync(item.Node, true);
            await sftpClient.ConnectAsync(CancellationToken.None);
            try
            {
                await using var stream = File.OpenWrite(fileToSave);
                await sftpClient.DownloadFileAsync(fileNameTarGz, stream);
            }
            finally
            {
                sftpClient.Disconnect();
            }
            sw.Stop();

            logger.LogDebug("Download tar.gz: {fileNameTarGz} to {fileToSave}, Time: {time}ms", fileNameTarGz, fileToSave, sw.ElapsedMilliseconds);
            logs.AppendLine($"[{item.Node}] Downloaded in {sw.Elapsed.TotalSeconds:F2} sec");

            // Delete tar.gz
            sw.Restart();
            cmd = $"rm {fileNameTarGz}";
            result = await clusterClient.SshExecuteAsync(item.Node, true, [cmd]);
            ret = result[0];
            sw.Stop();

            logger.LogDebug("Delete tar.gz: {cmd}, Result: {exitCode}, Time: {time}ms", cmd, ret.ExitCode, sw.ElapsedMilliseconds);
            logs.AppendLine($"[{item.Node}] Deleted tar.gz in {sw.Elapsed.TotalSeconds:F2} sec");

            files.Add(new(item.Node, logs.ToString(), fileToSave));
        }

        totalSw.Stop();
        logger.LogDebug("Backup completed in {totalSeconds:F2} seconds", totalSw.Elapsed.TotalSeconds);

        return files;
    }
}
