/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Service;

namespace Corsinvest.ProxmoxVE.Admin.Core.Support.RequestSupport;

public partial class RenderSupport
{
    [Inject] private IOptionsSnapshot<AdminOptions> AdminOptions { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    private string WhoUsing { get; set; } = default!;
    private string Info { get; set; } = default!;

    private async Task ClusterNameChanged(string clustername)
    {
        Info = "";
        WhoUsing = "";
        var clusterOptions = PveClientService.GetClusterOptions(clustername)!;
        var client = await PveClientService.GetClient(clusterOptions);
        if (client != null)
        {
            WhoUsing = await PveAdminHelper.GenerateWhoUsing(client, AdminOptions.Value);
            Info = await PveAdminHelper.GeClusterInfo(client, clusterOptions);
        }
    }
}