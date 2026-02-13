/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Net.Mime;
using BlazorDownloadFile;
using Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Services;

namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Components;

public partial class Scans(IBlazorDownloadFileService blazorDownloadFileService,
                             IDbContextFactory<ModuleDbContext> dbContextFactory,
                             IBackgroundJobService backgroundJobService,
                             ISettingsService settingsService,
                             DialogService dialogService,
                             NotificationService notificationService,
                             IDiagnosticService diagnosticService,
                             EventNotificationService eventNotificationService) : IClusterName,
                                                                                  IRefreshableData,
                                                                                  IDisposable
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private Settings Settings { get; set; } = new();
    private IList<Data> SelectedItems { get; set; } = [];
    private bool InDownload { get; set; }
    private RadzenDataGrid<Data> DataGridRef { get; set; } = default!;
    private ResultLoadData<Data> ResultLoadData { get; set; } = new(null!, -1, null);

    private record Data(int Id,
                         int Warning,
                         int Critical,
                         int Info,
                         DateTime Start,
                         DateTime? End)
    {
        public TimeSpan Duration
            => End.HasValue
                ? (End - Start).Value
                : TimeSpan.Zero;
    }

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

    private async Task LoadDataAsync(LoadDataArgs args)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        ResultLoadData = await DataGridRef.LoadDataAsync(db.JobResults.FromClusterName(ClusterName),
                                                         args,
                                                         a => new Data(a.Id,
                                                                       a.Warning,
                                                                       a.Critical,
                                                                       a.Info,
                                                                       a.Start,
                                                                       a.End),
                                                         ResultLoadData.Filter);
    }

    public async Task RefreshDataAsync()
    {
        Settings = settingsService.GetForModule<Module, Settings>(ClusterName);
        if (DataGridRef != null) { await InvokeAsync(DataGridRef.Reload); }
    }

    private async Task DeleteAsync()
    {
        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Delete selected row"], true))
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            await db.JobResults.DeleteAsync(SelectedItems[0].Id);
            await eventNotificationService.PublishAsync(new DataChangedNotification());
            //await DataGridRef.Reload();
            notificationService.Info(L["Row has been deleted!"]);
        }
    }

    private async Task KeyDownAsync(KeyboardEventArgs e)
    {
        if (SelectedItems.Any() && e.IsForDelete()) { await DeleteAsync(); }
        else if (SelectedItems.Any() && e.IsForEdit()) { }
        else if (e.IsForNew()) { }
    }

    private async Task DownloadAsync()
    {
        InDownload = true;

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var result = (await db.JobResults.Include(a => a.Details).FromIdAsync(SelectedItems[0].Id))!;
        await using var ms = diagnosticService.GeneratePdf(result);
        await blazorDownloadFileService.DownloadFile($"Diagnostic-{result.ClusterName}-{result.Start}.pdf", ms, MediaTypeNames.Application.Pdf);

        InDownload = false;
    }

    private void Scan()
    {
        backgroundJobService.Schedule<Job>(a => a.ScanFromResultAsync(ClusterName), TimeSpan.FromSeconds(5));
        notificationService.Info(L["Scan started!"]);
    }

    public void Dispose() => eventNotificationService.Unsubscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
}
