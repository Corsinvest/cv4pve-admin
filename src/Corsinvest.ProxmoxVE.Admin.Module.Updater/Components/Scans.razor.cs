/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Net.Mime;
using BlazorDownloadFile;
using Corsinvest.ProxmoxVE.Admin.Module.Updater.Helpers;
using Corsinvest.ProxmoxVE.Admin.Module.Updater.Models;
using Corsinvest.ProxmoxVE.Admin.Module.Updater.Services;

namespace Corsinvest.ProxmoxVE.Admin.Module.Updater.Components;

public partial class Scans(IAdminService adminService,
                         IBackgroundJobService backgroundJobService,
                         ISettingsService settingsService,
                         IBlazorDownloadFileService blazorDownloadFileService,
                         NotificationService notificationService,
                         IUpdaterService updaterService,
                         EventNotificationService eventNotificationService) : IRefreshableData,
                                                                              IClusterName,
                                                                              IDisposable
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private IEnumerable<ClusterResourceUpdateScanInfo> Items { get; set; } = [];
    private IList<ClusterResourceUpdateScanInfo> SelectedItems { get; set; } = [];
    private Settings Settings { get; set; } = new();
    private bool IsLoading { get; set; }
    private bool InDownload { get; set; }

    protected override async Task OnInitializedAsync()
    {
        eventNotificationService.Subscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        await RefreshDataAsync();
    }

    private async Task HandleDataChangedNotificationAsync(DataChangedNotification notification)
    {
        await RefreshDataAsync();
        await InvokeAsync(StateHasChanged);
    }

    public async Task RefreshDataAsync()
    {
        Settings = settingsService.GetForModule<Module, Settings>(ClusterName);

        IsLoading = true;
        Items = await ActionHelper.GetAsync(adminService[ClusterName]);
        IsLoading = false;

        await InvokeAsync(StateHasChanged);
    }

    private static bool ShowWatingUpdate(ClusterResourceUpdateScanInfo item) => item.UpdateScanStatus == UpdateInfoStatus.InScan;

    private async Task DownloadAsync()
    {
        InDownload = true;

        var start = Items.Min(a => a.UpdateScanTimestamp)!;
        await using var ms = updaterService.GeneratePdf(ClusterName, Items);
        await blazorDownloadFileService.DownloadFile($"Update-{ClusterName}-{start}.pdf", ms, MediaTypeNames.Application.Pdf);

        InDownload = false;
    }

    private static void RowRender(RowRenderEventArgs<ClusterResourceUpdateScanInfo> args)
    {
        switch (args.Data!.UpdateScanStatus)
        {
            case UpdateInfoStatus.InError: args.SetRowStyleError(); break;
            case UpdateInfoStatus.Ok: break;
            case UpdateInfoStatus.InScan: break;
            default: break;
        }
    }

    private void Scan()
    {
        backgroundJobService.Schedule<Job>(a => a.ScanFromResultAsync(ClusterName), TimeSpan.FromSeconds(5));
        notificationService.Info(L["Scan started!"]);
    }

    public void Dispose() => eventNotificationService.Unsubscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
}
