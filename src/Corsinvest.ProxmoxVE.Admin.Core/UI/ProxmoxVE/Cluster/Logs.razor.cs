/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Contracts;
using Corsinvest.AppHero.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Cluster;

public partial class Logs : IRefreshable
{
    [EditorRequired][Parameter] public PveClient PveClient { get; set; } = default!;
    [Parameter] public PermissionsRead Permissions { get; set; } = default!;

    [Inject] private IDataGridManager<ClusterLog> DataGridManager { get; set; } = default!;

    public async Task Refresh() => await DataGridManager.Refresh();

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Cluster Log"];
        DataGridManager.DefaultSort = new() { [nameof(ClusterLog.Time)] = true };
        DataGridManager.QueryAsync = async () => await PveClient.Cluster.Log.Get(100);

    }
}