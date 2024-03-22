/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Repository;
using Corsinvest.ProxmoxVE.Admin.Core.Extensions;
using Corsinvest.ProxmoxVE.Admin.VzDumpTrend.Repository;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension;
using Humanizer.Bytes;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Corsinvest.ProxmoxVE.Admin.VzDumpTrend;

internal class Helper
{
    public static DateTime ParseDateBackup(string value) => DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", null);

    private static readonly string[] _sizes = ["KIB", "MIB", "GIB", "TIB"];

    public static List<VzDumpDetail> ParserVzDumpFromTaskLog(VzDumpTask task)
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

        var backups = new List<VzDumpDetail>();
        var rows = task.Log!.SplitNewLine();

        VzDumpDetail backup = null!;
        var start = task.Start;
        var isIncrementally = false;

        for (int i = 0; i < rows.Length; i++)
        {
            var row = rows[i];
            if (row.StartsWith(KEY_STARTING_ERROR))
            {
                var vmId = row[KEY_STARTING_ERROR.Length..].Split(' ')[0];
                if (backup != null && backup.VmId != vmId)
                {
                    backup = new VzDumpDetail
                    {
                        Task = task,
                        VmId = vmId,
                        Logs = [],
                        Error = row
                    };
                    backups.Add(backup);
                }
            }
            else if (row.StartsWith(KEY_STARTING))
            {
                backup = new VzDumpDetail
                {
                    Task = task,
                    VmId = row[KEY_STARTING.Length..].Split(' ')[0],
                    Logs = []
                };
                backups.Add(backup);
            }
            else if (backup != null)
            {
                if (row.StartsWith(KEY_STARTED))
                {
                    backup.Start = ParseDateBackup(row[KEY_STARTED.Length..]);
                    start = backup.Start.Value;
                }
                else if (row.StartsWith(KEY_FINISHED))
                {
                    backup.End = ParseDateBackup(row[KEY_FINISHED.Length..]);
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
                        value = double.Parse(data[0]) * Math.Pow(2, indexSize * 10) + " B";
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

            backup?.Logs.Add(row);
        }

        return backups;
    }

    private static async Task PopulateDb(PveClient client,
                                         ModuleClusterOptions moduleClusterOptions,
                                         IRepository<VzDumpTask> dumpTaskRepo,
                                         bool all,
                                         string clusterName)
    {
        const string KEY_STORAGE = "--storage ";

        foreach (var node in (await client.GetNodes()).Where(a => a.IsOnline))
        {
            //list task backup
            foreach (var taskItem in await client.Nodes[node.Node].Tasks.Get(typefilter: "vzdump")) //, limit: 9999
            {
                var task = await dumpTaskRepo.FirstOrDefaultAsync(new VzDumpTaskSpec(clusterName, taskItem.UniqueTaskId));
                if (task != null)
                {
                    if (all)
                    {
                        await dumpTaskRepo.DeleteAsync(task);
                    }
                    else
                    {
                        break;
                    }
                }

                var logLines = await client.Nodes[node.Node].Tasks[taskItem.UniqueTaskId].Log.Get(limit: 10000);
                var rowStorage = logLines.Where(a => a.StartsWith("INFO: starting new backup job: vzdump")).FirstOrDefault();
                var storage = "";
                if (rowStorage != null)
                {
                    var posStorage = rowStorage.IndexOf(KEY_STORAGE);
                    if (posStorage > 0) { storage = rowStorage[(posStorage + KEY_STORAGE.Length)..].Split(" ")[0]; }
                }

                task = new VzDumpTask
                {
                    ClusterName = clusterName,
                    Start = taskItem.StartTimeDate,
                    End = taskItem.EndTimeDate,
                    TaskId = taskItem.UniqueTaskId,
                    Status = taskItem.Status,
                    Node = node.Node,
                    Storage = storage,
                    Log = string.Join(Environment.NewLine, logLines),
                };

                task.Details.AddRange(ParserVzDumpFromTaskLog(task));
                await dumpTaskRepo.AddAsync(task);
            }

            //list task backup delete
            foreach (var taskItem in await client.Nodes[node.Node].Tasks.Get(typefilter: "imgdel", limit: 1000))
            {
                var logLines = (await client.Nodes[node.Node].Tasks[taskItem.UniqueTaskId].Log.Get(limit: 10000))
                                     .Where(a => a.StartsWith("Removed volume "));

                foreach (var item in logLines) { var data = item[17..]; }
            }
        }

        //remove old logs       
        if (await dumpTaskRepo.CountAsync(new VzDumpTaskSpec(clusterName)) > 0)
        {
            var maxDate = (await dumpTaskRepo.ListAsync(new VzDumpTaskSpec(clusterName)))
                                .Max(a => a.Start)
                                .AddDays(-moduleClusterOptions.MaxDaysLogs);
            var tasks = await dumpTaskRepo.ListAsync(new VzDumpTaskSpec(clusterName).Over(maxDate));
            await dumpTaskRepo.DeleteRangeAsync(tasks);
        }
    }

    public static async Task Scan(IServiceScope scope, string clusterName)
    {
        var client = await scope.GetPveClient(clusterName);
        if (client == null) { return; }

        var loggerFactory = scope.GetLoggerFactory();
        var logger = loggerFactory.CreateLogger(typeof(Helper));

        var dumpTasksRepo = scope.GetRepository<VzDumpTask>();
        var moduleClusterOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<Options>>().Value.Get(clusterName);

        using (logger.LogTimeOperation(LogLevel.Information, true, "Collect backup data cluster '{clusterName}'", clusterName))
        {
            await PopulateDb(client, moduleClusterOptions, dumpTasksRepo, false, clusterName);
        }
    }
}