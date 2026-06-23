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
        var lockedRows = new List<Data>();

        foreach (var clusterClient in AdminService.GetFrom(ClusterNames))
        {
            var lockedVms = (await clusterClient.CachedData.GetResourcesAsync(false))
                                .Where(a => a.ResourceType == ClusterResourceType.Vm && a.IsLocked);

            foreach (var vm in lockedVms)
            {
                lockedRows.Add(new Data(
                    $"VM {vm.VmId} — {vm.Lock}",
                    1,
                    UrlHelper.Resources.VmUrl(vm.VmId, clusterClient.Settings.Name)));
            }
        }

        Items = lockedRows;
        Count = lockedRows.Count;
        Status = Count > 0 ? WidgetState.Issues : WidgetState.Ok;
    }
}
