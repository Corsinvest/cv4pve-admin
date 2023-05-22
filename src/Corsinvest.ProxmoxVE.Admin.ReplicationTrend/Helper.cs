/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Repository;
using Corsinvest.ProxmoxVE.Admin.Core.Extensions;
using Corsinvest.ProxmoxVE.Admin.ReplicationTrend.Repository;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension;
using Humanizer.Bytes;
using Microsoft.Extensions.Logging;
using Nextended.Core.Extensions;
using System.Globalization;

namespace Corsinvest.ProxmoxVE.Admin.ReplicationTrend;

internal class Helper
{
    public static async Task Scan(IServiceScope scope, string clusterName)
    {
        var client = await scope.GetPveClient(clusterName);
        if (client == null) { return; }

        var loggerFactory = scope.GetLoggerFactory();
        var logger = loggerFactory.CreateLogger(typeof(Helper));

        var replicationResultRepo = scope.GetRepository<ReplicationResult>();
        var moduleClusterOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<Options>>().Value.Get(clusterName);

        using (logger.LogTimeOperation(LogLevel.Information, true, "Collect replcation data"))
        {
            await PopulateDb(client, moduleClusterOptions, replicationResultRepo, clusterName);
        }
    }

    private static async Task PopulateDb(PveClient client,
                                         ModuleClusterOptions moduleClusterOptions,
                                         IRepository<ReplicationResult> replicationResultRepo,
                                         string clusterName)
    {
        const string KEY_SIZE = ": total estimated size is";

        static DateTime ParseDateTime(string value) => DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", null);

        var list = new List<ReplicationResult>();

        foreach (var node in (await client.GetNodes()).Where(a => a.IsOnline))
        {
            foreach (var job in await client.Nodes[node.Node].Replication.Get())
            {
                var rows = (await client.Nodes[node.Node].Replication[job.Id].Log.ReadJobLog())
                            .ToEnumerable()
                            .OrderBy(a => a.n)
                            .Select(a => a.t as string)
                            .ToArray();

                var lastSync = DateTimeOffset.FromUnixTimeMilliseconds(job.LastSync).DateTime;

                if (!await replicationResultRepo.AnyAsync(new ReplicationResultSpec(clusterName).Exists(job.Id, lastSync)))
                {
                    list.Add(new()
                    {
                        ClusterName = clusterName,
                        JobId = job.Id,
                        VmId = job.Guest,
                        Start = ParseDateTime(rows[0]!.Substring(0, 19)),
                        End = ParseDateTime(rows[^1]!.Substring(0, 19)),
                        Log = string.Join(Environment.NewLine, rows),
                        LastSync = lastSync,
                        Error = job.Error,
                        Status = string.IsNullOrWhiteSpace(job.Error),
                        Duration = job.Duration,
                        Size = rows.Where(a => a!.Contains(KEY_SIZE))
                                   .Select(a => ByteSize.Parse(a![(a!.IndexOf(KEY_SIZE) + KEY_SIZE.Length)..]
                                                               .EnsureEndsWith("B")
                                                               .Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                                                        .Bytes)
                                   .Sum()
                    });
                }
            }
        }

        await replicationResultRepo.AddRangeAsync(list);

        //remove old logs       
        if (await replicationResultRepo.CountAsync(new ReplicationResultSpec(clusterName)) > 0)
        {
            var maxDate = (await replicationResultRepo.ListAsync(new ReplicationResultSpec(clusterName)))
                                .Max(a => a.Start)
                                .AddDays(-moduleClusterOptions.MaxDaysLogs);
            var tasks = await replicationResultRepo.ListAsync(new ReplicationResultSpec(clusterName).Over(maxDate));
            await replicationResultRepo.DeleteRangeAsync(tasks);
        }
    }
}