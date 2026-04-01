/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components.Widgets.Maps;

public partial class ClusterDetailDialog(IAdminService adminService)
{
    [Parameter] public string ClusterName { get; set; } = default!;

    private bool Initialized { get; set; }
    private IEnumerable<ResourceUsageItem> DataUsages { get; set; } = [];
    private IReadOnlyList<Detail> Details { get; set; } = [];

    private record Detail(string Type, string Status, int Count);

    protected override async Task OnInitializedAsync()
    {
        var clusterClient = adminService[ClusterName];

        DataUsages = await clusterClient.GetResourceUsage(L, true);

        Details = [.. (await clusterClient.CachedData.GetResourcesAsync(false))
                    .Where(a => a.ResourceType is ClusterResourceType.Vm or ClusterResourceType.Node or ClusterResourceType.Storage)
                    .OrderBy(a => a.ResourceType)
                    .ThenBy(a => a.Type)
                    .GroupBy(a => new { a.Type, a.Status })
                    .Select(a => new Detail(a.Key.Type, a.Key.Status, a.Count()))];

        Initialized = true;
    }
}
