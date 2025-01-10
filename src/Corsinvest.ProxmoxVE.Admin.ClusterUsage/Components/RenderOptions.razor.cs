/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.ClusterUsage.Components;

public partial class RenderOptions
{
    [Inject] private IJobService JobService { get; set; } = default!;

    public override async Task SaveAsync()
    {
        foreach (var item in Options.Clusters)
        {
            JobService.ScheduleOrRemove<Job>(a => a.ScanAsync(item.ClusterName), item.CronExpression, item.Enabled, item.ClusterName);
        }

        await base.SaveAsync();
    }
}
