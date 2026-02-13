/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Components.Widgets;

public class Size(IAdminService adminService,
                  ISettingsService settingsService,
                  ILoggerFactory loggerFactory,
                  EventNotificationService eventNotificationService) : WidgetSparklineBase<Size.Data, object>
{
    public record Data(DateTime Date, double Size);

    protected override async Task OnInitializedAsync()
    {
        SerieTitle = L["Size"];
        CategoryProperty = nameof(Data.Date);
        ValueProperty = nameof(Data.Size);
        ValueFormatter = value => FormatHelper.FromBytes((double)value);

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
        var items = new List<Data>();

        foreach (var clusterClient in adminService.Where(a => ClusterNames.Contains(a.Settings.Name), ClusterNames.Any()))
        {
            if (!clusterClient.Settings.AllowCalculateSnapshotSize) { continue; }

            var settings = settingsService.GetForModule<Module, Settings>(clusterClient.Settings.Name);

            var data = await ActionHelper.GetInfoAsync(await clusterClient.GetPveClientAsync(),
                                                       settings,
                                                       loggerFactory,
                                                       ActionHelper.AllVms);

            var disks = await clusterClient.CachedData.GetDisksInfoAsync(false);

            foreach (var item in data)
            {
                item.SnapshotsSize = DiskInfoHelper.CalculateSnapshots(item.VmId, item.Name, disks);

                if (item.SnapshotsSize > 0)
                {
                    items.Add(new(item.Date.Date, item.SnapshotsSize));
                }
            }
        }

        Items = [.. items.GroupBy(a => a.Date)
                         .Select(a => new Data(a.Key, a.Sum(b => b.Size)))
                         .OrderBy(a => a.Date)];

        var lastDate = items.OrderByDescending(a => a.Date).FirstOrDefault()?.Date;
        MakeFooterText($"{L["Last"]}: {(lastDate.HasValue ? lastDate.Value.ToLocalTime().ToString("g") : string.Empty)}");
    }

    public override void Dispose()
    {
        eventNotificationService.Unsubscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        base.Dispose();
    }
}
