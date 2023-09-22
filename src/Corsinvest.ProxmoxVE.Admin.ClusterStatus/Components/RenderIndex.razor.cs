/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Contracts;
using Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Cluster;
using Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Common;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;
using ClasterStatusModel = Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster.ClusterStatus;

namespace Corsinvest.ProxmoxVE.Admin.ClusterStatus.Components;

public partial class RenderIndex : IRefreshable
{
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    private PveClient PveClient { get; set; } = default!;
    private Summary? RefSummary { get; set; } = default!;
    private Resources? RefResources { get; set; } = default!;
    private Tasks? RefTasks { get; set; } = default!;
    private Logs? RefLogs { get; set; } = default!;

    public async Task Refresh()
    {
        await RefSummary!.Refresh();
        await RefResources!.Refresh();

        if (RefTasks != null) { await RefTasks.Refresh(); }
        if (RefLogs != null) { await RefLogs.Refresh(); }
    }

    protected override async Task OnInitializedAsync()
    {
        try { PveClient = await PveClientService.GetClientCurrentClusterAsync(); }
        catch { }
    }

    private async Task<IEnumerable<ClusterResource>> GetResources() => (await PveClient.GetResources(ClusterResourceType.All)).CalculateHostUsage();
    private async Task<IEnumerable<ClasterStatusModel>> GetStatus() => await PveClient.Cluster.Status.Get();
    private async Task<string?> GetCephStatus()
    {
        var result = await PveClient.Cluster.Ceph.Status.Status();
        return result.IsSuccessStatusCode
                ? result.Response.data.health.status
                : null;
    }

    private async Task<IEnumerable<NodeTask>> GetTasks() => await PveClient.Cluster.Tasks.Get();
}