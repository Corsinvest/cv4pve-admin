using System.Globalization;
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Api;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace Corsinvest.ProxmoxVE.Admin.Module.BackupAnalytics.Helpers;

internal class ActionHelper : BaseActionHelper<Module, Settings, DataChangedNotification>
{
    public static DateTime ParseDateBackup(string value) => DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", null);

    private static readonly string[] _sizes = ["KIB", "MIB", "GIB", "TIB"];

    public static List<JobResult> ParseLog(TaskResult job)
    {
        const string KEY_STARTING = "INFO: Starting Backup of VM ";
        const string KEY_STARTING_ERROR = "ERROR: Backup of VM ";
        const string KEY_ENDING = "INFO: Finished Backup of VM ";
        const string KEY_STARTED = "INFO: Backup started at ";
        const string KEY_FINISHED = "INFO: Backup finished at ";
        const string KEY_ARCHIVE_SIZE = "INFO: archive file size: ";
        const string KEY_FAILED = "INFO: Failed at ";
        const string KEY_ERROR = "ERROR: ";
        const string KEY_TRANSFERRED = "INFO: transferred ";
        const string KEY_ARCHIVE = "INFO: creating archive '";
        const string KEY_IS_INCREMENTALLY = "INFO: backup was done incrementally,";

        var backups = new List<JobResult>();
        var rows = job.Logs!.SplitNewLine();

        JobResult backup = null!;
        var start = job.Start;
        var isIncrementally = false;

        for (var i = 0; i < rows.Length; i++)
        {
            var row = rows[i];
            if (row.StartsWith(KEY_STARTING_ERROR))
            {
                var vmId = row[KEY_STARTING_ERROR.Length..].Split(' ')[0];
                if (backup != null && backup.VmId != vmId)
                {
                    backup = new()
                    {
                        //Task = task,
                        VmId = vmId,
                        Error = row
                    };
                    backups.Add(backup);
                }
            }
            else if (row.StartsWith(KEY_STARTING))
            {
                backup = new()
                {
                    //Task = task,
                    VmId = row[KEY_STARTING.Length..].Split(' ')[0]
                };
                backups.Add(backup);
            }
            else if (backup != null)
            {
                if (row.StartsWith(KEY_STARTED))
                {
                    backup.Start = DateTime.SpecifyKind(ParseDateBackup(row[KEY_STARTED.Length..]), DateTimeKind.Local).ToUniversalTime();
                    start = backup.Start.Value;
                }
                else if (row.StartsWith(KEY_FINISHED))
                {
                    backup.End = DateTime.SpecifyKind(ParseDateBackup(row[KEY_FINISHED.Length..]), DateTimeKind.Local).ToUniversalTime();
                }
                else if (row.StartsWith(KEY_ENDING))
                {
                    backup.Status = true;

                    if (!backup.Start.HasValue) { backup.Start = start; }

                    var time = row[KEY_ENDING.Length..].Split(' ')[1][1..^1].Split(':');
                    start = start.AddHours(int.Parse(time[0]))
                                 .AddMinutes(int.Parse(time[1]))
                                 .AddSeconds(int.Parse(time[2]));

                    if (!backup.End.HasValue) { backup.End = start; }
                }
                else if (row.StartsWith(KEY_ARCHIVE_SIZE))
                {
                    backup.Size = ByteSize.Parse(row[KEY_ARCHIVE_SIZE.Length..]
                                                    .Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                                                    .Bytes;
                }
                else if (row.StartsWith(KEY_IS_INCREMENTALLY)) { isIncrementally = true; }
                else if (row.StartsWith(KEY_FAILED))
                {
                    backup.Status = false;
                    backup.Start = ParseDateBackup(row[KEY_FAILED.Length..]);
                    backup.End = backup.Start;
                    backups.Add(backup);
                }
                else if (row.StartsWith(KEY_ERROR))
                {
                    if (!string.IsNullOrWhiteSpace(backup.Error)) { backup.Error += Environment.NewLine; }
                    backup.Error += row[KEY_ERROR.Length..];
                }
                else if (row.StartsWith(KEY_TRANSFERRED))
                {
                    var value = row[KEY_TRANSFERRED.Length..];
                    value = value[..value.IndexOf("in")]
                                 .Trim()
                                 .Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

                    var data = value.Split(" ");
                    var indexSize = _sizes.IndexOf(data[1].ToUpper());
                    if (indexSize >= 0)
                    {
                        indexSize++;
                        value = (double.Parse(data[0]) * Math.Pow(2, indexSize * 10)) + " B";
                    }

                    backup.TransferSize = ByteSize.Parse(value).Bytes;

                    //PBS incremental backup
                    if (isIncrementally) { backup.Size = backup.TransferSize; }
                }
                else if (row.StartsWith(KEY_ARCHIVE))
                {
                    backup.Archive = row[KEY_ARCHIVE.Length..][0..^1];
                }
            }

            if (backup != null)
            {
                backup.Logs += row + Environment.NewLine;
            }
        }

        return backups;
    }

    private static async Task<(int TaskCount, int JobCount)> ScanAsync(PveClient client,
                                                                        Settings settings,
                                                                        ModuleDbContext db,
                                                                        bool all,
                                                                        ILogger logger)
    {
        const string KEY_STORAGE = "--storage ";
        var taskCount = 0;
        var jobCount = 0;

        foreach (var node in (await client.GetNodesAsync()).Where(a => a.IsOnline))
        {
            //list task backup
            var taskItems = await client.Nodes[node.Node].Tasks.GetAsync(typefilter: "vzdump"); //, limit: 9999

            // Batch optimization: Load all existing tasks for this node in one query
            var taskIds = taskItems.Select(t => t.UniqueTaskId).ToList();
            var existingTasks = new Dictionary<string, TaskResult>();

            // Process in batches to avoid SQL parameter limits
            const int batchSize = 1000;
            for (var i = 0; i < taskIds.Count; i += batchSize)
            {
                var batch = taskIds.Skip(i).Take(batchSize).ToList();
                var batchResults = await db.TaskResults
                                           .FromClusterName(settings.ClusterName)
                                           .Where(t => t.TaskId != null && batch.Contains(t.TaskId))
                                           .ToDictionaryAsync(t => t.TaskId!);

                foreach (var kvp in batchResults)
                {
                    existingTasks[kvp.Key] = kvp.Value;
                }
            }

            foreach (var taskItem in taskItems)
            {
                var task = existingTasks.GetValueOrDefault(taskItem.UniqueTaskId);
                if (task != null)
                {
                    if (all)
                    {
                        await db.TaskResults.DeleteAsync(task.Id);
                    }
                    else
                    {
                        break;
                    }
                }

                var logLines = await client.Nodes[node.Node].Tasks[taskItem.UniqueTaskId].Log.GetAsync(limit: 10000);
                var rowStorage = logLines.FirstOrDefault(a => a.StartsWith("INFO: starting new backup job: vzdump"));
                var storage = string.Empty;
                if (rowStorage != null)
                {
                    var posStorage = rowStorage.IndexOf(KEY_STORAGE);
                    if (posStorage > 0) { storage = rowStorage[(posStorage + KEY_STORAGE.Length)..].Split(" ")[0]; }
                }

                task = new()
                {
                    ClusterName = settings.ClusterName,
                    Start = DateTime.SpecifyKind(taskItem.StartTimeDate, DateTimeKind.Local).ToUniversalTime(),
                    End = DateTime.SpecifyKind(taskItem.EndTimeDate, DateTimeKind.Local).ToUniversalTime(),
                    TaskId = taskItem.UniqueTaskId,
                    Status = taskItem.Status,
                    Node = node.Node,
                    Storage = storage,
                    Logs = string.Join(Environment.NewLine, logLines)
                };

                try
                {
                    task.Jobs.AddRange(ParseLog(task));

                    await db.TaskResults.AddAsync(task);
                    taskCount++;
                    jobCount += task.Jobs.Count;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error importing data for {ClusterName} TaskId {TaskId}", settings.ClusterName, taskItem.UniqueTaskId);
                }
            }

            await db.SaveChangesAsync();
        }

        //remove old logs
        var minDate = DateTime.UtcNow.AddDays(-settings.MaxDaysLogs);

        await db.TaskResults.FromClusterName(settings.ClusterName)
                            .Where(a => a.Start < minDate)
                            .ExecuteDeleteAsync();

        return (taskCount, jobCount);
    }

    public static async Task ScanAsync(IServiceScope scope, string clusterName, bool automatic)
    {
        var logger = scope.GetLoggerFactory().CreateLogger<ActionHelper>();
        var auditService = scope.GetAuditService();

        using (logger.LogTimeOperation(LogLevel.Information, true, "Collect backup data for cluster '{clusterName}'", clusterName))
        {
            await using var db = await scope.GetDbContextAsync<ModuleDbContext>();
            var (taskCount, jobCount) = await ScanAsync(await scope.GetClusterClient(clusterName).GetPveClientAsync(),
                                                        GetModuleSettings(scope, clusterName),
                                                        db,
                                                        false,
                                                        logger);

            await auditService.LogAsync("BackupAnalytics.Scan", true, $"Cluster: {clusterName}, Tasks: {taskCount}, Jobs: {jobCount}");

            await PublishDataChangedAsync(scope);
        }
    }
}
