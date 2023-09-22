/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.DisksStatus.Components;

public partial class RenderIndex
{
    [Inject] private IPveClientService PveClientService { get; set; } = default!;
    private PveClient PveClient { get; set; } = default!;
    protected override async Task OnInitializedAsync()
    {
        try
        {
            PveClient = await PveClientService.GetClientCurrentClusterAsync();
        }
        catch { }
    }

    public async Task<IEnumerable<ClusterResource>> GetNodes() => (await PveClient.GetResources(ClusterResourceType.Node)).Where(a => a.IsOnline);

}
