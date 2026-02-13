/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Globalization;
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Api;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace Corsinvest.ProxmoxVE.Admin.Module.ReplicationAnalytics.Helpers;

internal class ActionHelper : BaseActionHelper<Module, Settings, DataChangedNotification>
{
    private static async Task<(int JobCount, int SuccessCount, int FailureCount)> ScanAsync(PveClient client,
                                                                                           Settings settings,
                                                                                           ModuleDbContext db,
                                                                                           ILogger logger)
    {
        const string KEY_SIZE = ": total estimated size is";
        var jobCount = 0;
        var successCount = 0;
        var failureCount = 0;

        static DateTime ParseDateTime(string value) => DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", null);

        foreach (var node in (await client.GetNodesAsync()).Where(a => a.IsOnline))
        {
            var jobs = await client.Nodes[node.Node].Replication.GetAsync();

            // Batch optimization: Load all existing jobs for this node in one query
            var jobKeys = jobs.Select(j => new
            {
                JobId = j.Id,
                LastSync = DateTimeOffset.FromUnixTimeSeconds(j.LastSync).UtcDateTime
            }).ToList();

            var existingJobs = new HashSet<(string JobId, DateTime LastSync)>();

            const int batchSize = 500; // Smaller batch due to composite key
            for (var i = 0; i < jobKeys.Count; i += batchSize)
            {
                var batch = jobKeys.Skip(i).Take(batchSize).ToList();
                var jobIds = batch.ConvertAll(b => b.JobId);
                var lastSyncs = batch.ConvertAll(b => b.LastSync);

                var batchResults = await db.JobResults
                                           .FromClusterName(settings.ClusterName)
                                           .Where(jr => jobIds.Contains(jr.JobId) && lastSyncs.Contains(jr.LastSync))
                                           .Select(jr => new { jr.JobId, jr.LastSync })
                                           .ToListAsync();

                foreach (var result in batchResults.Where(r => batch.Any(b => b.JobId == r.JobId && b.LastSync == r.LastSync)))
                {
                    existingJobs.Add((result.JobId, result.LastSync));
                }
            }

            foreach (var job in jobs)
            {
                var rows = (await client.Nodes[node.Node].Replication[job.Id].Log.ReadJobLog())
                            .ToEnumerable()
                            .OrderBy(a => a.n)
                            .Select(a => a.t as string)
                            .ToArray();

                var lastSync = DateTimeOffset.FromUnixTimeSeconds(job.LastSync).UtcDateTime;

                if (!existingJobs.Contains((job.Id, lastSync)))
                {
                    try
                    {
                        var status = string.IsNullOrWhiteSpace(job.Error);
                        await db.JobResults.AddAsync(new()
                        {
                            ClusterName = settings.ClusterName,
                            JobId = job.Id,
                            VmId = job.Guest,
                            Logs = string.Join(Environment.NewLine, rows),
                            LastSync = lastSync,
                            Error = job.Error,
                            Source = job.Source,
                            Target = job.Target,
                            Status = status,
                            //Duration = job.Duration,
                            Start = DateTime.SpecifyKind(ParseDateTime(rows[0]![..19]), DateTimeKind.Local).ToUniversalTime(),
                            End = DateTime.SpecifyKind(ParseDateTime(rows[^1]![..19]), DateTimeKind.Local).ToUniversalTime(),
                            Size = rows.Where(a => a!.Contains(KEY_SIZE))
                                       .Sum(a => ByteSize.Parse(a![(a!.IndexOf(KEY_SIZE) + KEY_SIZE.Length)..]
                                                                    .EnsureEndsWith(ByteSize.ByteSymbol)
                                                                    .Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                                                                    .Bytes)
                        });

                        jobCount++;
                        if (status) { successCount++; } else { failureCount++; }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error importing data for {ClusterName} JobId {JobId}", settings.ClusterName, job.Id);
                    }
                }
            }
        }

        await db.SaveChangesAsync();

        //remove old logs
        var maxDate = db.JobResults
                        .FromClusterName(settings.ClusterName)
                        .Select(a => a.Start)
                        .Max()
                        .AddDays(-settings.MaxDaysLogs);

        await db.JobResults.FromClusterName(settings.ClusterName)
                           .Where(a => a.Start < maxDate)
                           .ExecuteDeleteAsync();

        return (jobCount, successCount, failureCount);
    }

    public static async Task ScanAsync(IServiceScope scope, string clusterName, bool automatic)
    {
        var logger = scope.GetLoggerFactory().CreateLogger<ActionHelper>();
        var auditService = scope.GetAuditService();
        await using var db = await scope.GetDbContextAsync<ModuleDbContext>();

        using (logger.LogTimeOperation(LogLevel.Information, true, "Collect replication data for cluster '{clusterName}'", clusterName))
        {
            var (jobCount, successCount, failureCount) = await ScanAsync(await scope.GetClusterClient(clusterName).GetPveClientAsync(),
                                                                          GetModuleSettings(scope, clusterName),
                                                                          db,
                                                                          logger);

            await auditService.LogAsync("ReplicationAnalytics.Scan", true, $"Cluster: {clusterName}, Jobs: {jobCount}, Success: {successCount}, Failures: {failureCount}");

            await PublishDataChangedAsync(scope);
        }
    }
}
