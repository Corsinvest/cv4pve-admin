/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Diagnostics;
using System.Text;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Helpers;

public static class BackupHelper
{
    public record InfoBackupFile(string Node, string Logs, string FileName);

    public static async Task<IEnumerable<InfoBackupFile>> CreateAsync(PveClient client,
                                                                      IEnumerable<string> paths,
                                                                      string directoryWork,
                                                                      ILogger logger)
    {
        var totalSw = Stopwatch.StartNew();
        var files = new List<InfoBackupFile>();
        var date = DateTime.Now;
        var fileNameTarGz = $"/tmp/{date:yyyy-MM-dd-HH-mm-ss}.tar.gz";

        foreach (var item in (await client.Cluster.Resources.GetAsync(ClusterResourceType.Node)).Where(a => a.IsOnline))
        {
            await using var webTerm = new PveWebTermClient(client, item.Node);
            await webTerm.ConnectAsync();

            var logs = new StringBuilder();

            // Create tar.gz
            var sw = Stopwatch.StartNew();
            logs.AppendLine($"[{item.Node}] Create file tar.gz: {fileNameTarGz}");
            var cmd = $"tar --one-file-system -cvzPf {fileNameTarGz} {paths.JoinAsString(" ")}";
            var ret = await webTerm.ExecuteCommandAsync(cmd);
            sw.Stop();

            logger.LogDebug("Create tar.gz: {cmd}", cmd);
            logger.LogDebug("Result: {exitCode}, Time: {time}ms", ret.ExitCode, sw.ElapsedMilliseconds);

            logs.Append(ret.StdOut);
            logs.AppendLine($"[{item.Node}] Created tar.gz in {sw.Elapsed.TotalSeconds:F2} sec");

            // Download file
            sw.Restart();
            var fileToSave = Path.Combine(directoryWork, $"{item.Node}-config.tar.gz");

            logs.AppendLine($"[{item.Node}] Download file tar.gz: {fileNameTarGz} to {fileToSave}");
            await using var stream = File.OpenWrite(fileToSave);
            await webTerm.DownloadFileAsync(fileNameTarGz, stream);
            sw.Stop();

            logger.LogDebug("Download tar.gz: {fileNameTarGz} to {fileToSave}, Time: {time}ms", fileNameTarGz, fileToSave, sw.ElapsedMilliseconds);
            logs.AppendLine($"[{item.Node}] Downloaded in {sw.Elapsed.TotalSeconds:F2} sec");

            // Delete tar.gz
            sw.Restart();
            cmd = $"rm {fileNameTarGz}";
            ret = await webTerm.ExecuteCommandAsync(cmd);
            sw.Stop();

            logger.LogDebug("Delete tar.gz: {cmd}, Result: {exitCode}, Time: {time}ms", cmd, ret.ExitCode, sw.ElapsedMilliseconds);
            logs.AppendLine($"[{item.Node}] Deleted tar.gz in {sw.Elapsed.TotalSeconds:F2} sec");

            files.Add(new(item.Node, logs.ToString(), fileToSave));
        }

        totalSw.Stop();
        logger.LogInformation("Backup completed in {totalSeconds:F2} seconds", totalSw.Elapsed.TotalSeconds);

        return files;
    }
}
