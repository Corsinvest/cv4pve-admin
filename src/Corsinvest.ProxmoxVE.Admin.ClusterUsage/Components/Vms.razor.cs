/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Cluster;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.ClusterUsage.Components;

public partial class Vms
{
    [Inject] private IPveClientService PveClientService { get; set; } = default!;


    private Resources? RefResources { get; set; } = default!;
    private PveClient PveClient { get; set; } = default!;
    private bool OnlyRun { get; set; } = true;

    protected override async Task OnInitializedAsync() => PveClient = await PveClientService.GetClientCurrentClusterAsync();

    private async Task OnlyRunChanged(bool value)
    {
        OnlyRun = value;
        await RefResources!.Refresh();
    }

    private async Task<IEnumerable<ClusterResource>> GetVms()
        => (await PveClient.GetResources(ClusterResourceType.All))
            .CalculateHostUsage()
            .Where(a => a.ResourceType == ClusterResourceType.Vm)
            .Where(a => a.IsRunning, OnlyRun);

    private async Task<IEnumerable<VmRrdData>> GetVmRrdData(long vmId, RrdDataTimeFrame rrdDataTimeFrame, RrdDataConsolidation rrdDataConsolidation)
        => await PveClient.GetVmRrdData(await PveClient.GetVm(vmId), rrdDataTimeFrame, rrdDataConsolidation);
}