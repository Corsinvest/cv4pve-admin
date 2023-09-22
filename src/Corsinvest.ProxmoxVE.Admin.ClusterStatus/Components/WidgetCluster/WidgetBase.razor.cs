/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Core.Models;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Microsoft.Extensions.Options;

namespace Corsinvest.ProxmoxVE.Admin.ClusterStatus.Components.WidgetCluster;

public partial class WidgetBase
{
    [Parameter] public int Index { get; set; }

    [Inject] private IPveClientService PveClientService { get; set; } = default!;
    [Inject] private IOptionsSnapshot<Options> Options { get; set; } = default!;

    private ResourceUsage? DataUsage { get; set; }
    private string Image { get; set; } = default!;
    private ThresholdPercentual Threshold { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var pveClient = await PveClientService.GetClientCurrentClusterAsync();
        var resources = (await pveClient.GetResources(ClusterResourceType.All))
                            .CalculateHostUsage();

        Image = new[] { PveBlazorHelper.Icons.Cpu, PveBlazorHelper.Icons.Memory, PveBlazorHelper.Icons.Storage }[Index];

        var thresholds = Options.Value.Get(await PveClientService.GetCurrentClusterNameAsync());
        Threshold = new[] { thresholds.Cpu, thresholds.Memory, thresholds.Storage }[Index];

        DataUsage = ResourceUsage.GetUsages(resources, L).ToArray()[Index];
    }
}