using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Helpers;
using Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Persistence;

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
        if (settings.Folder.Enabled)
        {
            var loggerFactory = scope.GetLoggerFactory();
            var logger = loggerFactory.CreateLogger(typeof(FolderHelper));
            var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();
            await using var db = await scope.GetDbContextAsync<ModuleDbContext>();

            using (logger.LogTimeOperation(LogLevel.Information, true, "NodeProtect: backup data to folder for cluster '{clusterName}'", clusterName))
            {
                var success = true;
                try
                {
                    await BackupAsync(scope.GetClusterClient(clusterName),
                                      settings,
                                      db,
                                      logger);
                }
                catch (Exception ex)
                {
                    success = false;
                    logger.LogError(ex, "Backup failed for cluster {ClusterName}", clusterName);
                    throw;
                }
                finally
                {
                    await auditService.LogAsync("NodeProtect.Folder.Backup", success, $"Cluster: {clusterName}, Paths: {settings.PathsToBackup.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length} items");
                }

                await scope.GetEventNotificationService().PublishAsync(new DataChangedNotification());
            }
        }
    }

    private static async Task BackupAsync(ClusterClient clusterClient,
                                          Settings settings,
                                          ModuleDbContext db,
                                          ILogger logger)
    {
        var taskId = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        var baseDir = GetDirectoryWork(settings.ClusterName);
        var directoryWork = Path.Combine(baseDir, taskId);
        Directory.CreateDirectory(directoryWork);

        var status = false;
        var start = DateTime.Now;

        IEnumerable<BackupHelper.InfoBackupFile> files = [];

        try
        {
            files = await BackupHelper.CreateAsync(await clusterClient.GetPveClientAsync(),
                                                   settings.PathsToBackup.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries),
                                                   directoryWork,
                                                   logger);
            status = true;
        }
        catch (Exception ex) { logger.LogError(ex, ex.Message); }

        var end = DateTime.Now;

        foreach (var item in files)
        {
            await db.FolderTaskResults.AddAsync(new()
            {
                TaskId = taskId,
                ClusterName = settings.ClusterName,
                Start = start.ToUniversalTime(),
                End = end.ToUniversalTime(),
                Status = status,
                FileName = Path.GetFileName(item.FileName),
                Node = item.Node,
                Logs = item.Logs,
                Size = File.Exists(item.FileName)
                        ? new System.IO.FileInfo(item.FileName).Length
                        : 0
            });

            await db.SaveChangesAsync();
        }

        //remove old
        foreach (var item in Directory.EnumerateDirectories(baseDir).OrderDescending().Skip(settings.Folder.Keep).ToArray())
        {
            Directory.Delete(item, true);
        }

        var maxDate = db.FolderTaskResults
                        .FromClusterName(settings.ClusterName)
                        .Select(a => a.Start)
                        .Distinct()
                        .OrderDescending()
                        .Skip(settings.Folder.Keep)
                        .FirstOrDefault();

        await db.FolderTaskResults.FromClusterName(settings.ClusterName)
                            .Where(a => a.Start < maxDate)
                            .ExecuteDeleteAsync();
    }

    public static void ConfigureService() => Directory.CreateDirectory(GetDirectoryWork());
}
