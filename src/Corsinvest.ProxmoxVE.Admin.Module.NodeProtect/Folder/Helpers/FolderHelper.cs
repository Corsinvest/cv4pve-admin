/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;
using Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Persistence;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.NodeProtect.Api;

namespace Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Folder.Helpers;

internal static class FolderHelper
{
    private static string GetDirectoryWork() => Path.Combine(new Module().PathData, "folder");

    public static string GetDirectoryWork(string clusterName)
    {
        PathValidationHelper.ValidatePathComponent(clusterName, nameof(clusterName));
        return Path.Combine(GetDirectoryWork(), clusterName);
    }

    public static string GetDirectoryWork(string clusterName, string taskId)
    {
        PathValidationHelper.ValidatePathComponent(clusterName, nameof(clusterName));
        PathValidationHelper.ValidatePathComponent(taskId, nameof(taskId));
        return Path.Combine(GetDirectoryWork(clusterName), taskId);
    }

    public static string GetPath(string clusterName, string taskId, string fileName)
    {
        PathValidationHelper.ValidatePathComponent(clusterName, nameof(clusterName));
        PathValidationHelper.ValidatePathComponent(taskId, nameof(taskId));
        PathValidationHelper.ValidatePathComponent(fileName, nameof(fileName));
        return Path.Combine(GetDirectoryWork(clusterName, taskId), fileName);
    }

    public static async Task BackupAsync(IServiceScope scope, string clusterName)
    {
        var settings = scope.GetSettingsService().GetForModule<Module, Settings>(clusterName);
        if (!settings.Folder.Enabled) { return; }

        var loggerFactory = scope.GetLoggerFactory();
        var logger = loggerFactory.CreateLogger(typeof(FolderHelper));
        var auditService = scope.GetRequiredService<IAuditService>();
        var taskTracker = scope.GetRequiredService<ITaskTrackerService>();
        var module = scope.GetModuleService().Get<Module>()!;
        await using var db = await scope.GetDbContextAsync<ModuleDbContext>();
        var clusterClient = scope.GetClusterClient(clusterName);
        var engine = new ProtectEngine(loggerFactory.CreateLogger<ProtectEngine>());

        await using var taskScope = await taskTracker.StartAsync($"NodeProtect Folder [{clusterName}]",
                                                                 clusterName,
                                                                 module.Name,
                                                                 "Folder",
                                                                 detailUrl: module.GetLinkByProvider("folder")!.GetRealUrl(clusterName));

        using (logger.LogTimeOperation(LogLevel.Information, true, "NodeProtect: backup data to folder for cluster '{clusterName}'", clusterName))
        {
            var success = true;
            try
            {
                var taskId = DateTime.Now.ToString(ProtectEngine.DateFormat);
                var baseDir = GetDirectoryWork(clusterName);
                var directoryWork = Path.Combine(baseDir, taskId);
                Directory.CreateDirectory(directoryWork);

                var paths = settings.PathsToBackup.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                var start = DateTime.Now;

                taskScope.Log($"Paths to backup: {paths.Length}");
                taskScope.Log($"Working directory: {directoryWork}");

                var nodes = (await clusterClient.CachedData.GetResourcesAsync(false))
                                .Where(a => a.ResourceType == ClusterResourceType.Node && a.IsOnline)
                                .ToList();

                taskScope.Log($"Online nodes: {nodes.Count} ({string.Join(", ", nodes.Select(n => n.Node))})");

                foreach (var node in nodes)
                {
                    var targetFile = Path.Combine(directoryWork, $"{node.Node}{ProtectEngine.FileNameSuffix}");
                    var nodeStatus = false;
                    var logs = string.Empty;
                    var connectionInfo = await clusterClient.GetSshConnectionInfoAsync(node.Node, true);
                    taskScope.Log($"Backing up node: {node.Node}");
                    try
                    {
                        logs = await engine.BackupNodeAsync(node.Node, connectionInfo, paths, targetFile);
                        nodeStatus = true;
                        var size = File.Exists(targetFile) ? new System.IO.FileInfo(targetFile).Length : 0;
                        taskScope.Log($"[{node.Node}] OK - Size: {size:N0} bytes");
                        taskScope.Log(logs);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Backup failed for node {Node}", node.Node);
                        logs = ex.Message;
                        taskScope.Log($"[{node.Node}] {ex.Message}", LogLevel.Error);
                    }

                    await db.FolderTaskResults.AddAsync(new()
                    {
                        TaskId = taskId,
                        ClusterName = clusterName,
                        Start = start.ToUniversalTime(),
                        End = DateTime.UtcNow,
                        Status = nodeStatus,
                        FileName = Path.GetFileName(targetFile),
                        Node = node.Node,
                        Logs = logs,
                        Size = File.Exists(targetFile) ? new System.IO.FileInfo(targetFile).Length : 0
                    });
                    await db.SaveChangesAsync();
                }

                foreach (var item in Directory.EnumerateDirectories(baseDir).OrderDescending().Skip(settings.Folder.Keep).ToArray())
                {
                    taskScope.Log($"Removing old backup: {Path.GetFileName(item)}");
                    Directory.Delete(item, true);
                }

                var maxDate = db.FolderTaskResults
                                .FromClusterName(clusterName)
                                .Select(a => a.Start)
                                .Distinct()
                                .OrderDescending()
                                .Skip(settings.Folder.Keep)
                                .FirstOrDefault();

                await db.FolderTaskResults.FromClusterName(clusterName)
                                    .Where(a => a.Start < maxDate)
                                    .ExecuteDeleteAsync();
            }
            catch (Exception ex)
            {
                success = false;
                logger.LogError(ex, "Backup failed for cluster {ClusterName}", clusterName);
                taskScope.Item.Status = TaskItemStatus.Failed;
                taskScope.Log(ex.ToString(), LogLevel.Error);
                throw;
            }
            finally
            {
                await auditService.LogAsync("NodeProtect.Folder.Backup",
                                            success,
                                            $"Cluster: {clusterName}, " +
                                            $"Paths: {settings.PathsToBackup.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length} items");
            }

            await scope.GetEventNotificationService().PublishAsync(new DataChangedNotification());
        }
    }

    public static void ConfigureService() => Directory.CreateDirectory(GetDirectoryWork());
}
