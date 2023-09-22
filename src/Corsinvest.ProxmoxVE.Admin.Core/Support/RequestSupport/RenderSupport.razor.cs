/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Support.RequestSupport;

public partial class RenderSupport
{
    [Inject] private IOptionsSnapshot<AdminOptions> AdminOptions { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    private string WhoUsing { get; set; } = string.Empty;
    private string Info { get; set; } = string.Empty;

    private async Task ClusterNameChanged(string clustername)
    {
        Info = string.Empty;
        WhoUsing = string.Empty;
        var clusterOptions = PveClientService.GetClusterOptions(clustername)!;
        var client = await PveClientService.GetClientAsync(clusterOptions);
        if (client != null)
        {
            WhoUsing = await PveAdminHelper.GenerateWhoUsing(client, AdminOptions.Value);
            Info = await PveAdminHelper.GetClusterInfo(client, clusterOptions);
        }
    }
}