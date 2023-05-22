/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.BackgroundJob;
using Corsinvest.ProxmoxVE.Admin.Core.Extensions;

namespace Corsinvest.ProxmoxVE.Admin.ReplicationTrend.Components;

public partial class RenderOptions
{
    [Inject] private IJobService JobService { get; set; } = default!;

    public override async Task SaveAsync()
    {
        foreach (var item in Options.Clusters)
        {
            JobService.ScheduleOrRemove<Job>(a => a.Scan(item.ClusterName), item.CronExpression, item.Enabled, item.ClusterName);
        }
        await base.SaveAsync();
    }
}