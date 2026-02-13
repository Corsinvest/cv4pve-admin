/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components.Widgets;

public partial class VmsLocked(IAdminService adminService, ISettingsService settingsService)
    : WidgetThumbDetailsBase<object>(adminService, settingsService)
{
    protected override async Task RefreshDataAsyncInt()
    {
        var allLockedVms = new List<ClusterResource>();

        foreach (var clusterClient in adminService.Where(a => ClusterNames.Contains(a.Settings.Name), ClusterNames.Any()))
        {
            var lockedVms = (await clusterClient.CachedData.GetResourcesAsync(false))
                                .Where(a => a.ResourceType == ClusterResourceType.Vm && a.IsLocked);

            allLockedVms.AddRange(lockedVms);
        }

        Items = [.. allLockedVms.GroupBy(a => a.Lock)
                                .Select(a => new Data(a.Key, a.Count()))];
    }
}
