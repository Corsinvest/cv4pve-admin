/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Extensions;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Diagnostic.Components;

public partial class RenderOptions
{
    [Inject] private IJobService JobService { get; set; } = default!;

    private string[] ThresholdTexts { get; } = new[] { "CPU", "Memory", "Network" };
    private string[] ThresholdIcons { get; } = new[] { PveBlazorHelper.Icons.Cpu, PveBlazorHelper.Icons.Memory, PveBlazorHelper.Icons.Network };

    public override async Task SaveAsync()
    {
        foreach (var item in Options.Clusters)
        {
            JobService.ScheduleOrRemove<Job>(a => a.Create(item.ClusterName), item.CronExpression, item.Enabled, item.ClusterName);
        }

        await base.SaveAsync();
    }

    private static string GetIconType(int index)
        => PveBlazorHelper.Icons.GetResourceType(new[] { PveConstants.KeyApiNode, PveConstants.KeyApiQemu, PveConstants.KeyApiLxc }[index]);
}