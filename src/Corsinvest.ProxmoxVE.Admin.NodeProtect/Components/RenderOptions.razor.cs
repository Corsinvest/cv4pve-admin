/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Extensions;

namespace Corsinvest.ProxmoxVE.Admin.NodeProtect.Components;

public partial class RenderOptions
{
    [Inject] private IJobService JobService { get; set; } = default!;

    public override async Task SaveAsync()
    {
        foreach (var item in Options.Clusters)
        {
            JobService.ScheduleOrRemove<Job>(a => a.Protect(item.ClusterName), item.CronExpression, item.Enabled, item.ClusterName);
        }

        await base.SaveAsync();
    }
}