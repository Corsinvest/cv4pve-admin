/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.Server;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.BackgroundJobs.Filters;

public class PreventConcurrentExecutionWithJobAndArgsFilter : JobFilterAttribute, IServerFilter
{
    private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
    private static readonly ConcurrentDictionary<string, Guid> JobLocks = new();

    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public void OnPerforming(PerformingContext context)
    {
        try
        {
            var job = context.BackgroundJob?.Job;
            if (job == null)
            {
                Logger.Warn("Job information is missing. Canceling execution.");
                context.Canceled = true;
                return;
            }

            var jobName = $"{job.Type.FullName}.{job.Method.Name}";
            var argsJson = JsonSerializer.Serialize(job.Args, _options);

            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(argsJson));
            var argsHash = Convert.ToBase64String(hashBytes);
            var lockKey = $"{jobName}-{argsHash}";

            if (!JobLocks.TryAdd(lockKey, Guid.NewGuid()))
            {
                Logger.Info($"Job '{jobName}' with same parameters is already running. Job ID: {context.BackgroundJob!.Id}");
                context.Canceled = true;
            }
            else
            {
                context.Items["LockKey"] = lockKey;
            }
        }
        catch (Exception ex)
        {
            Logger.ErrorException("Error while trying to prevent concurrent job execution.", ex);
            context.Canceled = true;
        }
    }

    public void OnPerformed(PerformedContext filterContext)
    {
        if (filterContext.Items.TryGetValue("LockKey", out var lockKey) && lockKey is string key)
        {
            JobLocks.TryRemove(key, out _);
        }
    }
}
