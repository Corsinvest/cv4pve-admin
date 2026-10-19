/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Cluster;

public partial class Backups(IAdminService adminService) : IClusterName
{
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;
    [Parameter] public string Style { get; set; } = default!;

    private RadzenDataGrid<ClusterBackup> DataGridRef { get; set; } = default!;
    private IEnumerable<ClusterBackup> Items { get; set; } = default!;
    protected override async Task OnInitializedAsync()
    {
        var client = await adminService[ClusterName].GetPveClientAsync();
        var items = new List<ClusterBackup>();

        // A job on all the guests, a pool or excluded ids is shown only to who can read the whole cluster
        foreach (var item in (await client.Cluster.Backup.GetAsync()).OrderBy(a => a.Id))
        {
            var selection = item.All || !string.IsNullOrWhiteSpace(item.Pool) ? string.Empty : item.VmId ?? string.Empty;
            if (await PermissionService.CanReadVmSelectionAsync(ClusterName, selection)) { items.Add(item); }
        }
        Items = items;
    }
}
