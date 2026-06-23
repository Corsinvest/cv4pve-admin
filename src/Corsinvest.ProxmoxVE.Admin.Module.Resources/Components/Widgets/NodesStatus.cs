/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components.Widgets;

public partial class NodesStatus(IAdminService adminService, ISettingsService settingsService)
    : WidgetThumbDetailsBase<object>(adminService, settingsService)
{
    protected override async Task RefreshDataAsyncInt()
    {
        var countOnline = 0;
        var offlineRows = new List<Data>();

        foreach (var clusterClient in AdminService.GetFrom(ClusterNames))
        {
            var nodes = (await clusterClient.CachedData.GetResourcesAsync(false))
                                .Where(a => a.ResourceType == ClusterResourceType.Node);

            countOnline += nodes.Count(a => a.IsOnline);

            foreach (var node in nodes.Where(a => !a.IsOnline))
            {
                offlineRows.Add(new Data(
                    $"{node.Node} ({node.Status})",
                    1,
                    UrlHelper.Resources.NodeUrl(node.Node, clusterClient.Settings.Name)));
            }
        }

        Items = offlineRows;
        Count = offlineRows.Count;
        Status = Count > 0 ? WidgetState.Issues : WidgetState.Ok;
        Message = countOnline > 0
                    ? L["{0} nodes online", countOnline]
                    : string.Empty;
    }
}
