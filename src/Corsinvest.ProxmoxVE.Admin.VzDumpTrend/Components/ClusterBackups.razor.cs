/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.VzDumpTrend.Components;

public partial class ClusterBackups
{
    [Parameter] public string Height { get; set; } = default!;
    [EditorRequired][Parameter] public PveClient PveClient { get; set; } = default!;

    [Inject] private IDataGridManager<ClusterBackup> DataGridManager { get; set; } = default!;

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["VZ Dump Trend"];
        DataGridManager.QueryAsync = async () => (await PveClient.Cluster.Backup.GetAsync()).OrderBy(a => a.Id);
    }
}