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
        var countOffline = 0;
        var allOfflineNodes = new List<ClusterResource>();

        foreach (var clusterClient in adminService.Where(a => ClusterNames.Contains(a.Settings.Name), ClusterNames.Any()))
        {
            var nodes = (await clusterClient.CachedData.GetResourcesAsync(false))
                                .Where(a => a.ResourceType == ClusterResourceType.Node);

            countOnline += nodes.Count(a => a.IsOnline);
            countOffline += nodes.Count(a => !a.IsOnline);

            allOfflineNodes.AddRange(nodes.Where(a => !a.IsOnline));
        }

        Items = [.. allOfflineNodes.GroupBy(a => a.Status)
                                   .Select(a => new Data(a.Key, a.Count()))];

        Message = countOnline > 0
                    ? L["{0} nodes online", countOnline]
                    : string.Empty;
    }
}
