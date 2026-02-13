/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;
using Corsinvest.ProxmoxVE.Admin.Module.Updater.Helpers;
using Corsinvest.ProxmoxVE.Admin.Module.Updater.Models;

namespace Corsinvest.ProxmoxVE.Admin.Module.Updater.Components.Widgets;

public class Status(IAdminService adminService,
                    EventNotificationService eventNotificationService) : WidgetDonutBase<object>
{
    protected override async Task OnInitializedAsync()
    {
        SerieTitle = L["VM / CT"];
        eventNotificationService.Subscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        await base.OnInitializedAsync();
    }

    private async Task HandleDataChangedNotificationAsync(DataChangedNotification notification)
    {
        await RefreshDataAsyncInt();
        await InvokeAsync(StateHasChanged);
    }

    protected override async Task RefreshDataAsyncInt()
    {
        Clear();
        DateTime? lastScan = null;

        var totalUpdated = 0;
        var totalToUpdate = 0;
        var totalInError = 0;

        foreach (var item in adminService.Where(a => ClusterNames.Contains(a.Settings.Name), ClusterNames.Any()))
        {
            var vms = await ActionHelper.GetAsync(item);

            totalUpdated += vms.Count(a => a.UpdateScanStatus == UpdateInfoStatus.Ok
                                            && !a.UpdateNormalAvailable
                                            && !a.UpdateSecurityAvailable
                                            && !a.UpdateRequireReboot);

            totalToUpdate += vms.Count(a => a.UpdateScanStatus == UpdateInfoStatus.Ok
                                             && (a.UpdateNormalAvailable || a.UpdateSecurityAvailable || a.UpdateRequireReboot));

            totalInError += vms.Count(a => a.UpdateScanStatus != UpdateInfoStatus.Ok && a.UpdateScanStatus != UpdateInfoStatus.InScan);

            var vmLastScan = vms.Where(a => a.UpdateScanTimestamp.HasValue)
                                .OrderByDescending(a => a.UpdateScanTimestamp)
                                .FirstOrDefault()?.UpdateScanTimestamp;
            if (vmLastScan.HasValue && (!lastScan.HasValue || vmLastScan > lastScan))
            {
                lastScan = vmLastScan;
            }
        }

        Set("Updated", totalUpdated, Colors.Success);
        Set("To update", totalToUpdate, Colors.Warning);
        Set("In error", totalInError, "var(--rz-error)");

        LastExecution = lastScan;
        MakeFooterText($"{L["Last"]}: {(LastExecution.HasValue ? LastExecution.Value.ToLocalTime().ToString("g") : string.Empty)}");
    }

    public override void Dispose()
    {
        eventNotificationService.Unsubscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        base.Dispose();
    }
}
