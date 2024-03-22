/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Ardalis.Specification;
using Corsinvest.AppHero.Core.Domain.Repository;
using Corsinvest.AppHero.Core.Extensions;
using Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Extensions;
using Corsinvest.ProxmoxVE.Admin.Core.Repository;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Extension.Utils;
using Corsinvest.ProxmoxVE.AutoSnap.Api;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Net;

namespace Corsinvest.ProxmoxVE.Admin.AutoSnap;

internal class Helper
{
    public static string AllVms { get; } = "@all";

    public static Application GetApp(PveClient client, ILoggerFactory loggerFactory, TextWriter log)
        => new(client, loggerFactory, log, false);

    public static string GetLabelFromName(string name, string timestampFormat) => Application.GetLabelFromName(name, timestampFormat);

    private static ModuleClusterOptions GetModuleClusterOptions(IServiceScope scope, string clusterName)
        => scope.GetModuleClusterOptions<Options, ModuleClusterOptions>(clusterName);

    private static async Task<AutoSnapJob?> GetAutoSnapJob(IReadRepositoryBase<AutoSnapJob> jobRepo, int id, ILogger logger)
    {
        var job = await jobRepo.FirstOrDefaultAsync(new AutoSnapJobSpec(id));
        if (job == null) { logger.LogWarning("Job Id = {Id} not exists!", id); }
        return job;
    }

    public static async Task Delete(IServiceScope scope, IEnumerable<int> ids)
    {
        var loggerFactory = scope.GetLoggerFactory();
        var logger = loggerFactory.CreateLogger(typeof(Helper));

        using (logger.LogTimeOperation(LogLevel.Information, true, "Delete snapshot {Ids}", ids.JoinAsString(",")))
        {
            var jobRepo = scope.GetRepository<AutoSnapJob>();

            //remove snapshot
            foreach (var id in ids)
            {
                var job = await GetAutoSnapJob(jobRepo, id, logger);
                if (job == null) { continue; }

                var moduleClusterOptions = GetModuleClusterOptions(scope, job.ClusterName);
                if (moduleClusterOptions.OnRemoveJobRemoveSnapshots) { await Purge(scope, id); }
                await jobRepo.DeleteAsync(job);
            }
        }
    }

    public static async Task Purge(IServiceScope scope, int id)
    {
        var loggerFactory = scope.GetLoggerFactory();
        var logger = loggerFactory.CreateLogger(typeof(Helper));

        using (logger.LogTimeOperation(LogLevel.Information, true, "Purge snapshot from Job {Id}", id))
        {
            var jobRepo = scope.GetReadRepository<AutoSnapJob>();
            var job = await GetAutoSnapJob(jobRepo, id, logger);
            if (job == null) { return; }

            var moduleClusterOptions = GetModuleClusterOptions(scope, job.ClusterName);
            var client = await scope.GetPveClient(job.ClusterName);

            using var log = new StringWriterEvent();
            var app = GetApp(client, loggerFactory, log);

            await app.Clean(job.VmIds, job.Label, 0, job.TimeoutSnapshot * 1000, moduleClusterOptions.TimestampFormat);
        }
    }

    public static async Task Create(IServiceScope scope, int id)
    {
        var loggerFactory = scope.GetLoggerFactory();
        var logger = loggerFactory.CreateLogger(typeof(Helper));

        using (logger.LogTimeOperation(LogLevel.Information, true, "Execution AutoSnap from Job {id}", id))
        {
            var jobRepo = scope.GetRepository<AutoSnapJob>();
            var job = await GetAutoSnapJob(jobRepo, id, logger);
            if (job == null)
            {
                logger.LogWarning("Job Id = {Id} not exists!", id);

                //old job not recognized
                //var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
                //jobService.RemoveIfExists<Job>(job.ClusterName, id);
                return;
            }

            var history = new AutoSnapJobHistory
            {
                Job = job,
                ClusterName = job.ClusterName,
                Log = "",
            };

            //log
            using var log = new StringWriterEvent();
            log.WritedData += (sender, e) =>
            {
                history.Log = log.ToString();
            };

            history.Start = DateTime.Now;

            var jobHistoryRepo = scope.GetRepository<AutoSnapJobHistory>();
            await jobHistoryRepo.AddAsync(history);

            logger.LogInformation("Execution AutoSnap Job: {Id}", id);

            var client = await scope.GetPveClient(job.ClusterName);
            var app = GetApp(client, loggerFactory, log);

            var statusEventOk = true;

            //event
            app.PhaseEvent += (sender, e) =>
            {
                var hooks = job.Hooks.Where(a => a.Enabled && e.Phase == a.Phase).OrderBy(a => a.Order);
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
                                    var ret = ExecuteHook(item, e.Environments);
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

            var moduleClusterOptions = GetModuleClusterOptions(scope, job.ClusterName);

            using (logger.LogTimeOperation(LogLevel.Debug, true, "Execution physical Snap"))
            {
                var retSnap = await app.Snap(job.VmIds,
                                             job.Label,
                                             job.Keep,
                                             job.VmStatus,
                                             job.TimeoutSnapshot * 1000,
                                             moduleClusterOptions.TimestampFormat,
                                             moduleClusterOptions.MaxPercentageStorage,
                                             job.OnlyRuns);
                history.Status = retSnap.Status;
            }

            if (!statusEventOk) { history.Status = false; }

            history.End = DateTime.Now;
            history.Log = log.ToString();

            await jobHistoryRepo.SaveChangesAsync();

            //keep history
            var histories = job.Histories.Where(a => a.Start < history.Start)
                                         .OrderByDescending(a => a.End)
                                         .Skip(moduleClusterOptions.KeepHistory + job.Keep)
                                         .ToArray();

            if (histories.Length != 0)
            {
                foreach (var item in histories) { job.Histories.Remove(item); }
                await jobRepo.SaveChangesAsync();
            }

            //send notification
            if (moduleClusterOptions.NotificationChannels?.Any() is true
                && (moduleClusterOptions.Notify == Notify.Allways || moduleClusterOptions.Notify == Notify.OnFailureOnly && !history.Status))
            {
                var L = scope.ServiceProvider.GetRequiredService<IStringLocalizer<Helper>>();

                await scope.GetNotificationService().SendAsync(moduleClusterOptions.NotificationChannels, new()
                {
                    Subject = L["{0} - AutoSnap Id {1} [{2}] of cluster {3}",
                                scope.GetAppOptions().Name,
                                id,
                                history.Status ? L["OK"] : L["KO"],
                                job.ClusterName],
                    Body = history.Log.ReplaceLineEndings("<br>")
                });
            }
        }
    }

    public static async Task<IEnumerable<AutoSnapInfo>> GetInfo(PveClient client,
                                                                ModuleClusterOptions moduleClusterOptions,
                                                                ILoggerFactory loggerFactory,
                                                                string vmIdsOrNames)
    {
        var ret = new List<AutoSnapInfo>();
        foreach (var item in await GetApp(client, loggerFactory, null!)
                                        .Status(vmIdsOrNames, null, moduleClusterOptions.TimestampFormat))
        {
            var snaposhots = item.Value.Where(a => !string.IsNullOrWhiteSpace(Application.GetLabelFromName(a.Name, moduleClusterOptions.TimestampFormat)));
            ret.AddRange(snaposhots.Select(a => new AutoSnapInfo()
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
                Label = Application.GetLabelFromName(a.Name, moduleClusterOptions.TimestampFormat)
            }));
        }
        return ret.OrderBy(a => a.Label);
    }

    public static HttpResponseMessage ExecuteHook(AutoSnapJobHook hook, IReadOnlyDictionary<string, string> environments)
    {
        using var handler = new HttpClientHandler
        {
            Credentials = string.IsNullOrWhiteSpace(hook.Username) ?
                            null :
                            new NetworkCredential(hook.Username, hook.Password),

            //ignore certificate
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        using var client = new HttpClient(handler);

        var dataStr = hook.Data;
        var dic = new Dictionary<string, string>();
        if (hook.DataIsKeyValue) { dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(hook.Data)!; }

        var url = hook.Url;

        //replace value environments
        foreach (var item in environments)
        {
            var keyReplace = $"%{item.Key}%";
            var valueReplace = item.Value;
            url = url.Replace(keyReplace, valueReplace);

            if (hook.DataIsKeyValue)
            {
                foreach (var key in dic.Keys)
                {
                    dic[key] = dic[key].Replace(keyReplace, valueReplace);
                }
            }
            else
            {
                dataStr = dataStr.Replace(keyReplace, valueReplace);
            }
        }

        var content = hook.DataIsKeyValue ?
                        (ByteArrayContent)new FormUrlEncodedContent(dic) :
                        new StringContent(dataStr);

        return client.Send(hook.HttpMethod switch
        {
            AutoSnapJobHookHttpMethod.Get => new HttpRequestMessage(HttpMethod.Get, url),
            AutoSnapJobHookHttpMethod.Post => new HttpRequestMessage(HttpMethod.Post, url) { Content = content },
            AutoSnapJobHookHttpMethod.Put => new HttpRequestMessage(HttpMethod.Put, url) { Content = content },
            _ => throw new InvalidEnumArgumentException(),
        });
    }

    public static async Task Delete(IServiceScope scope, IEnumerable<AutoSnapInfo> snapshots, string clusterName)
    {
        var client = await scope.GetPveClient(clusterName);
        foreach (var item in snapshots)
        {
            await SnapshotHelper.RemoveSnapshot(client, item.Node, item.VmType, item.VmId, item.Name, 30000, true);
        }
    }

    public static async Task<string> GetVmIdsOrNames(IReadRepository<AutoSnapJob> jobRepo, string clusterName, bool enabled)
    {
        var specJob = new AutoSnapJobSpec(clusterName);
        if (enabled) { specJob = specJob.Enabled(); }
        return (await jobRepo.ListAsync(specJob)).Select(a => a.VmIds).JoinAsString(",");
    }

    public static async Task<(int scheduled, DateTime? last, int snapCount, int vmsScheduled, int inError)>
        Info(IServiceScopeFactory ServiceScopeFactory, string clusterName)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var client = await scope.GetPveClient(clusterName);
        var jobRepo = scope.GetReadRepository<AutoSnapJob>();
        var jobHistoryRepo = scope.GetReadRepository<AutoSnapJobHistory>();
        var moduleClusterOptions = GetModuleClusterOptions(scope, clusterName);
        var loggerFactory = scope.GetLoggerFactory();

        var vmIdsOrNames = await GetVmIdsOrNames(jobRepo, clusterName, true);
        var vmsCount = string.IsNullOrWhiteSpace(vmIdsOrNames) ?
                        0 :
                        (await client.GetVms(vmIdsOrNames)).Where(a => !a.IsUnknown).Count();

        var snapCount = moduleClusterOptions.SearchMode == SearchMode.Managed
                            ? vmsCount
                            : (await GetApp(client, loggerFactory, null!)
                                    .Status(AllVms, null, moduleClusterOptions.TimestampFormat))
                                    .Count;

        var specJob = new AutoSnapJobSpec(clusterName).Enabled();
        var jobCount = await jobRepo.CountAsync(specJob);

        var spec = new AutoSnapJobHistorySpec(clusterName);
        var start = (await jobHistoryRepo.FirstOrDefaultAsync(spec))?.Start;
        var inError = await jobHistoryRepo.CountAsync(spec.InError(7));
        return (jobCount, start, snapCount, vmsCount, inError);
    }
}