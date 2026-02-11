using System.ComponentModel;
using System.Text.Json;
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Vm;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.AutoSnap.Api;
using Microsoft.Extensions.Localization;

namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Helpers;

internal class ActionHelper : BaseActionHelper<Module, Settings, DataChangedNotification>
{
    public static string AllVms { get; } = "@all";

    public static Application GetApp(PveClient client, ILoggerFactory loggerFactory, TextWriter log)
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

                await auditService.LogAsync("AutoSnap.DeleteJob", true, $"Job ID: {id}, Cluster: {job.ClusterName}, Label: {job.Label}, VMs: {job.VmIdsList.JoinAsString(",")}");
            }
            await db.Jobs.DeleteAsync(id);
            await PublishDataChangedAsync(scope);
        }
    }

    public static async Task PurgeAsync(IServiceScope scope, int id)
    {
        var logger = scope.GetLoggerFactory().CreateLogger<ActionHelper>();
        var auditService = scope.GetAuditService();
        await using var db = await scope.GetDbContextAsync<ModuleDbContext>();

        using (logger.LogTimeOperation(LogLevel.Information, true, "Purge snapshot from Job {Id}", id))
        {
            var job = await db.Jobs.FromIdAsync(id);
            if (job == null) { return; }

            var settings = GetModuleClusterSettings(scope, job.ClusterName);
            var client = await scope.GetClusterClient(job.ClusterName).GetPveClientAsync();

            var success = true;
            try
            {
                await using var log = new StringWriterEvent();
                var app = GetApp(client, scope.GetLoggerFactory(), log);

                await app.CleanAsync(job.VmIdsList.JoinAsString(","),
                                     job.Label,
                                     0,
                                     job.TimeoutSnapshot * 1000,
                                     settings.TimestampFormat);
            }
            catch (Exception ex)
            {
                success = false;
                logger.LogError(ex, "Purge failed for Job ID {Id}", id);
                throw;
            }
            finally
            {
                await auditService.LogAsync("AutoSnap.PurgeSnapshots", success, $"Job ID: {id}, Cluster: {job.ClusterName}, Label: {job.Label}, VMs: {job.VmIdsList.Count()}");
            }

            await PublishDataChangedAsync(scope);
        }
    }

    public static async Task SnapAsync(IServiceScope scope, int id)
    {
        var logger = scope.GetLoggerFactory().CreateLogger<ActionHelper>();
        var auditService = scope.GetAuditService();
        await using var db = await scope.GetDbContextAsync<ModuleDbContext>();

        using (logger.LogTimeOperation(LogLevel.Information, true, "Execution AutoSnap from Job {id}", id))
        {
            var job = await db.Jobs.FromIdAsync(id);
            if (job == null)
            {
                logger.LogWarning("Job Id = {Id} does not exist!", id);
                await auditService.LogAsync("AutoSnap.Snap", false, $"Job ID: {id} not found");

                //old job not recognized
                //var backgroundJobService = scope.GetRequiredService<IJobService>();
                //backgroundJobService.RemoveIfExists<Job>(job.ClusterName, id);
                return;
            }

            var result = new JobResult
            {
                Job = job,
                Logs = string.Empty
            };

            //log
            await using var log = new StringWriterEvent();
            log.WritedData += (_, _) => result.Logs = log.ToString();

            result.Start = DateTime.UtcNow;

            logger.LogInformation("Execution AutoSnap Job: {Id}", id);

            var client = await scope.GetClusterClient(job.ClusterName).GetPveClientAsync();
            var app = GetApp(client, scope.GetLoggerFactory(), log);

            var statusEventOk = true;

            //event
            app.PhaseEvent += (_, e) =>
            {
                var hooks = job.WebHooks.Where(a => a.Enabled && e.Phase == a.Phase).OrderBy(a => a.OrderIndex);
                if (hooks.Any())
                {
                    using (logger.LogTimeOperation(LogLevel.Information, true, "Hook HTTP Phase '{Phase}'", e.Phase))
                    {
                        //call hook
                        foreach (var item in hooks)
                        {
                            using (logger.LogTimeOperation(LogLevel.Debug, true, "Execute HTTP Phase '{Phase}' Command '{Description}'", item.Phase, item.Description))
                            {
                                log.WriteLine($"Execute Hook HTTP '{item.Phase}' Command '{item.Description}'");
                                try
                                {
                                    var ret = ExecuteWebHook(item, e.Environments);
                                    log.WriteLine($"  Status Code '{ret.StatusCode}'");
                                    log.WriteLine($"  Reason Phrase '{ret.ReasonPhrase}'");
                                }
                                catch (Exception ex)
                                {
                                    statusEventOk = false;
                                    logger.LogError(ex, ex.Message);
                                    log.WriteLine($"  Error '{ex.Message}'");
                                }
                            }
                        }
                    }
                }
            };

            var settings = GetModuleClusterSettings(scope, job.ClusterName);

            using (logger.LogTimeOperation(LogLevel.Debug, true, "Execution physical Snap"))
            {
                var retSnap = await app.SnapAsync(job.VmIdsList.JoinAsString(","),
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

            await db.Results.AddAsync(result);
            await db.SaveChangesAsync();

            //keep history
            await db.Results.Where(a => a.Job.Id == id && a.Start < result.Start)
                            .OrderByDescending(a => a.End)
                            .Skip(settings.KeepHistory + job.Keep)
                            .ExecuteDeleteAsync();

            //send notification
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

            // Audit log
            await auditService.LogAsync("AutoSnap.Snap", result.Status, $"Job ID: {id}, Cluster: {job.ClusterName}, Label: {job.Label}, VMs: {job.VmIdsList.Count()}, Snap: {result.SnapName}, Duration: {result.Duration:hh':'mm':'ss}");

            await PublishDataChangedAsync(scope);
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
            var snaposhots = item.Value.Where(a => !string.IsNullOrWhiteSpace(Application.GetLabelFromName(a.Name, moduleClusterSettings.TimestampFormat)));
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
                Label = Application.GetLabelFromName(a.Name, moduleClusterSettings.TimestampFormat)
            }));
        }
        return ret.OrderBy(a => a.Label);
    }

    private static HttpResponseMessage ExecuteWebHook(JobWebHook webHook, IReadOnlyDictionary<string, string> environments)
    {
        using var handler = new HttpClientHandler();
        if (webHook.IgnoreSslCertificate) { handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true; }

        using var client = new HttpClient(handler);

        var body = webHook.Body;
        var header = webHook.Header;
        var endpoint = webHook.Url;

        //replace value environments
        foreach (var item in environments.Select(a => new { Key = $"%{a.Key}%", a.Value }))
        {
            endpoint = endpoint.Replace(item.Key, item.Value);
            body = body.Replace(item.Key, item.Value);
            header = header.Replace(item.Key, item.Value);
        }

        var content = new StringContent(body);
        var request = webHook.Method switch
        {
            AutoSnapJobHookHttpMethod.Get => new HttpRequestMessage(HttpMethod.Get, endpoint),
            AutoSnapJobHookHttpMethod.Post => new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content },
            AutoSnapJobHookHttpMethod.Put => new HttpRequestMessage(HttpMethod.Put, endpoint) { Content = content },
            _ => throw new InvalidEnumArgumentException()
        };

        foreach (var item in JsonSerializer.Deserialize<Dictionary<string, string>>(header)!)
        {
            request.Headers.Add(item.Key, item.Value);
        }

        return client.Send(request);
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

            await auditService.LogAsync("AutoSnap.DeleteSnapshots", true, $"Cluster: {clusterName}, Count: {snapshotList.Count}, Snapshots: {snapshotList.Select(s => $"{s.VmId}:{s.Name}").JoinAsString(", ")}");
        }

        await PublishDataChangedAsync(scope);
    }

    //public static async Task<(int scheduled, DateTime? last, int snapCount, int vmsScheduled, int inError)>
    //    InfoAsync(IServiceScopeFactory ServiceScopeFactory, string clusterName)
    //{
    //    using var scope = ServiceScopeFactory.CreateScope();
    //    var client = await scope.GetPveClientAsync(clusterName);
    //    var jobRepo = scope.GetReadRepository<AutoSnapJob>();
    //    var jobHistoryRepo = scope.GetReadRepository<AutoSnapJobHistory>();
    //    var moduleClusterOptions = GetModuleClusterOptions(scope, clusterName);
    //    var loggerFactory = scope.GetLoggerFactory();

    //    var vmIdsOrNames = await GetVmIdsOrNamesAsync(jobRepo, clusterName, true);
    //    var vmsCount = string.IsNullOrWhiteSpace(vmIdsOrNames) ?
    //                    0 :
    //                    (await client.GetVmsAsync(vmIdsOrNames)).Where(a => !a.IsUnknown).Count();

    //    var snapCount = moduleClusterOptions.SearchMode == SearchMode.Managed
    //                        ? vmsCount
    //                        : (await GetApp(client, loggerFactory, null!)
    //                                .StatusAsync(AllVms, null, moduleClusterOptions.TimestampFormat))
    //                                .Count;

    //    var specJob = new AutoSnapJobSpec(clusterName).Enabled();
    //    var jobCount = await jobRepo.CountAsync(specJob);

    //    var spec = new AutoSnapJobHistorySpec(clusterName);
    //    var start = (await jobHistoryRepo.FirstOrDefaultAsync(spec))?.Start;
    //    var inError = await jobHistoryRepo.CountAsync(spec.InError(7));
    //    return (jobCount, start, snapCount, vmsCount, inError);
    //}
}
