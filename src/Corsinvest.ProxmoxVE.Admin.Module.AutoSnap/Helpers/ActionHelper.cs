/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Vm;
using Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;
using Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Services;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.AutoSnap.Api;
using Microsoft.Extensions.Localization;

namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Helpers;

internal class ActionHelper : BaseActionHelper<Module, Settings, DataChangedNotification>
{
    public static string AllVms { get; } = "@all";

    public static AutoSnapEngine GetApp(PveClient client, ILoggerFactory loggerFactory, TextWriter log)
        => new(client, loggerFactory, log, false);

    private static Settings GetModuleClusterSettings(IServiceScope scope, string clusterName)
        => GetModuleSettings(scope, clusterName);

    public static async Task DeleteAsync(IServiceScope scope, int id)
    {
        var logger = scope.GetLoggerFactory().CreateLogger<ActionHelper>();
        var auditService = scope.GetAuditService();
        await using var db = await scope.GetDbContextAsync<ModuleDbContext>();

        using (logger.LogTimeOperation(LogLevel.Information, true, "Delete snapshot {Id}", id))
        {
            //remove snapshot
            var job = await db.Jobs.FromIdAsync(id);
            if (job != null)
            {
                var settings = GetModuleClusterSettings(scope, job.ClusterName);
                if (settings.OnRemoveJobRemoveSnapshots) { await PurgeAsync(scope, id); }

                await auditService.LogAsync("AutoSnap.DeleteJob",
                                            true,
                                            $"Job ID: {id}" +
                                            $", Cluster: {job.ClusterName}, " +
                                            $"Label: {job.Label}, " +
                                            $"VMs: {job.VmIds}");
            }
            await db.Jobs.DeleteAsync(id);
            await PublishDataChangedAsync(scope);
        }
    }

    public static async Task PurgeAsync(IServiceScope scope, int id)
    {
        var logger = scope.GetLoggerFactory().CreateLogger<ActionHelper>();
        var auditService = scope.GetAuditService();
        var taskTracker = scope.GetRequiredService<ITaskTrackerService>();
        await using var db = await scope.GetDbContextAsync<ModuleDbContext>();

        var job = await db.Jobs.FromIdAsync(id);
        if (job == null) { return; }

        await using var taskScope = await taskTracker.StartAsync($"AutoSnap purge [{job.ClusterName}] Job {id}", job.ClusterName, GetModule(scope).Name, id.ToString());
        using (logger.LogTimeOperation(LogLevel.Information, true, "Purge snapshot from Job {Id}", id))
        {
            var settings = GetModuleClusterSettings(scope, job.ClusterName);
            var client = await scope.GetClusterClient(job.ClusterName).GetPveClientAsync();

            var success = true;
            try
            {
                taskScope.Item.Phase = "Purging snapshots";

                await using var log = new StringWriterEvent();
                var app = GetApp(client, scope.GetLoggerFactory(), log);

                await app.CleanAsync(job.VmIds,
                                     job.Label,
                                     0,
                                     job.TimeoutSnapshot * 1000,
                                     settings.TimestampFormat);
            }
            catch (Exception ex)
            {
                success = false;
                taskScope.Item.Status = TaskItemStatus.Failed;
                taskScope.Log(ex.ToString(), LogLevel.Error);
                logger.LogError(ex, "Purge failed for Job ID {Id}", id);
                throw;
            }
            finally
            {
                await auditService.LogAsync("AutoSnap.PurgeSnapshots",
                                            success,
                                            $"Job ID: {id}, " +
                                            $"Cluster: {job.ClusterName}, " +
                                            $"Label: {job.Label}, " +
                                            $"VMs: {job.VmIds.Split(',').Length}");
            }

            await PublishDataChangedAsync(scope);
        }
    }

    public static async Task SnapAsync(IServiceScope scope, int id)
    {
        var logger = scope.GetLoggerFactory().CreateLogger<ActionHelper>();
        var auditService = scope.GetAuditService();
        var taskTracker = scope.GetRequiredService<ITaskTrackerService>();
        await using var db = await scope.GetDbContextAsync<ModuleDbContext>();

        var job = await db.Jobs.FromIdAsync(id);
        if (job == null)
        {
            logger.LogWarning("Job Id = {Id} does not exist!", id);
            await auditService.LogAsync("AutoSnap.Snap", false, $"Job ID: {id} not found");
            return;
        }

        await using var taskScope = await taskTracker.StartAsync($"AutoSnap [{job.ClusterName}] Job {id} ({job.Label})", job.ClusterName, GetModule(scope).Name, id.ToString());
        try
        {
            using (logger.LogTimeOperation(LogLevel.Information, true, "Execution AutoSnap from Job {id}", id))
            {
                var result = new JobResult
                {
                    Job = job,
                    Logs = string.Empty
                };

                await using var log = new StringWriterEvent();
                log.WritedData += (_, _) => result.Logs = log.ToString();

                result.Start = DateTime.UtcNow;
                logger.LogInformation("Execution AutoSnap Job: {Id}", id);

                var client = await scope.GetClusterClient(job.ClusterName).GetPveClientAsync();
                var app = GetApp(client, scope.GetLoggerFactory(), log);

                var statusEventOk = true;
                var hookService = scope.GetService<IAutoSnapHookService>();
                if (hookService != null)
                {
                    app.PhaseEvent += async (e) =>
                    {
                        try
                        {
                            await hookService.ExecuteAsync(job, e, log);
                        }
                        catch (Exception ex)
                        {
                            statusEventOk = false;
                            logger.LogError(ex, ex.Message);
                            log.WriteLine($"  Error '{ex.Message}'");
                        }
                    };
                }

                app.PhaseEvent += (e) =>
                {
                    taskScope.Item.Phase = e.Phase.ToString();
                    return Task.CompletedTask;
                };

                var settings = GetModuleClusterSettings(scope, job.ClusterName);

                taskScope.Item.Phase = "Taking snapshots";

                using (logger.LogTimeOperation(LogLevel.Debug, true, "Execution physical Snap"))
                {
                    var retSnap = await app.SnapAsync(job.VmIds,
                                                      job.Label,
                                                      job.Keep,
                                                      job.VmStatus,
                                                      job.TimeoutSnapshot * 1000,
                                                      settings.TimestampFormat,
                                                      settings.MaxPercentageStorage,
                                                      job.OnlyRuns);

                    result.SnapName = retSnap.SnapName;
                    result.Status = retSnap.Status;
                }

                if (!statusEventOk) { result.Status = false; }

                result.End = DateTime.UtcNow;
                result.Logs = log.ToString();

                taskScope.Item.Phase = "Saving results";

                await db.Results.AddAsync(result);
                await db.SaveChangesAsync();
                taskScope.Item.ReferenceId = result.Id.ToString();
                taskScope.Item.DetailUrl = GetModule(scope).LinkMain?.GetRealUrl(job.ClusterName);

                await db.Results.Where(a => a.Job.Id == id && a.Start < result.Start)
                                .OrderByDescending(a => a.End)
                                .Skip(settings.KeepHistory + job.Keep)
                                .ExecuteDeleteAsync();

                //send notification
                taskScope.Item.Phase = "Sending notifications";

                if (settings.NotifierConfigurations?.Any() is true
                    && settings.Notify is Notify.Allways or Notify.OnFailureOnly
                    && !result.Status)
                {
                    var L = scope.GetRequiredService<IStringLocalizer<ActionHelper>>();
                    var appSettings = scope.GetSettingsService().GetAppSettings();

                    await scope.GetNotifierService().SendAsync(settings.NotifierConfigurations, new()
                    {
                        Subject = L["{0} - AutoSnap Id {1} [{2}] of cluster {3}",
                                    appSettings.AppName,
                                    id,
                                    result.Status ? L["OK"] : L["KO"],
                                    job.ClusterName],
                        Body = result.Logs.ReplaceLineEndings("<br>")
                    });
                }

                await auditService.LogAsync("AutoSnap.Snap",
                                            result.Status,
                                            $"Job ID: {id}, " +
                                            $"Cluster: {job.ClusterName}, " +
                                            $"Label: {job.Label}, " +
                                            $"VMs: {job.VmIds.Split(',').Length}, " +
                                            $"Snap: {result.SnapName}, " +
                                            $"Duration: {result.Duration:hh':'mm':'ss}");

                taskScope.Log($"Job {id}, Cluster: {job.ClusterName}, Label: {job.Label}, Snap: {result.SnapName}, Status: {result.Status}");
                await PublishDataChangedAsync(scope);
            }
        }
        catch (Exception ex)
        {
            taskScope.Item.Status = TaskItemStatus.Failed;
            taskScope.Log(ex.ToString(), LogLevel.Error);
            throw;
        }
    }

    public static async Task<IEnumerable<AutoSnapInfo>> GetInfoAsync(PveClient client,
                                                                     Settings moduleClusterSettings,
                                                                     ILoggerFactory loggerFactory,
                                                                     string vmIdsOrNames)
    {
        var ret = new List<AutoSnapInfo>();
        foreach (var item in await GetApp(client, loggerFactory, null!).StatusAsync(vmIdsOrNames, null!, moduleClusterSettings.TimestampFormat))
        {
            var snaposhots = item.Value.Where(a => !string.IsNullOrWhiteSpace(AutoSnapEngine.GetLabelFromName(a.Name, moduleClusterSettings.TimestampFormat)));
            ret.AddRange(snaposhots.Select(a => new AutoSnapInfo
            {
                Description = a.Description,
                Name = a.Name,
                Node = item.Key.Node,
                Parent = a.Parent,
                Date = a.Date,
                VmId = item.Key.VmId,
                VmName = item.Key.Name,
                VmType = item.Key.VmType,
                VmStatus = a.VmStatus,
                Label = AutoSnapEngine.GetLabelFromName(a.Name, moduleClusterSettings.TimestampFormat)
            }));
        }
        return ret.OrderBy(a => a.Label);
    }

    public static async Task DeleteAsync(IServiceScope scope, IEnumerable<AutoSnapInfo> snapshots, string clusterName)
    {
        var logger = scope.GetLoggerFactory().CreateLogger<ActionHelper>();
        var auditService = scope.GetAuditService();
        var commandExecutor = scope.GetCommandExecutor();

        using (logger.LogTimeOperation(LogLevel.Information, true, "Execution Delete snapshots"))
        {
            var snapshotList = snapshots.ToList();

            foreach (var item in snapshotList)
            {
                logger.LogInformation("Execution Delete snapshot: {name}", item.Name);
                await commandExecutor.ExecuteAsync(new VmRemoveSnapshotCommand(clusterName, item.VmId, item.Name, Force: true));
            }

            await auditService.LogAsync("AutoSnap.DeleteSnapshots",
                                        true,
                                        $"Cluster: {clusterName}, " +
                                        $"Count: {snapshotList.Count}, " +
                                        $"Snapshots: {snapshotList.Select(s => $"{s.VmId}:{s.Name}").JoinAsString(", ")}");
        }

        await PublishDataChangedAsync(scope);
    }
}
