/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Radzen.Blazor;

namespace Corsinvest.ProxmoxVE.Admin.Module.ReplicationAnalytics.Components;

public partial class Replications(IDbContextFactory<ModuleDbContext> dbContextFactory,
                                  IBackgroundJobService backgroundJobService,
                                  ISettingsService settingsService,
                                  DialogService dialogService,
                                  NotificationService notificationService,
                                  EventNotificationService eventNotificationService) : IRefreshableData,
                                                                                       IClusterName,
                                                                                       IDisposable
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private RadzenDataGrid<Data> DataGridRef { get; set; } = default!;
    private ResultLoadData<Data> ResultLoadData { get; set; } = new(null!, -1, null);
    private Settings Settings { get; set; } = new();
    private GridLoader<JobResult, Data>? _loader;

    private class Data : JobResult;

    protected override async Task OnInitializedAsync()
    {
        eventNotificationService.Subscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        await RefreshDataAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            _loader = GridLoader.Create<JobResult, Data>(DataGridRef, defaultOrderBy: "Start desc");
        }
    }

    private async Task HandleDataChangedNotificationAsync(DataChangedNotification notification)
    {
        await RefreshDataAsync();
        await InvokeAsync(StateHasChanged);
    }

    public async Task RefreshDataAsync()
    {
        Settings = settingsService.GetForModule<Module, Settings>(ClusterName);
        if (_loader is not null) { await _loader.RefreshAsync(); }
        else if (DataGridRef != null) { await InvokeAsync(DataGridRef.Reload); }
    }

    private async Task LoadDataAsync(LoadDataArgs args)
    {
        if (_loader is null) { return; }
        await using var db = await dbContextFactory.CreateDbContextAsync();
        ResultLoadData = await _loader.LoadAsync(db.JobResults.FromClusterName(ClusterName),
                                                 args,
                                                 a => new Data
                                                 {
                                                     JobId = a.JobId,
                                                     VmId = a.VmId,
                                                     Start = a.Start,
                                                     End = a.End,
                                                     Source = a.Source,
                                                     Target = a.Target,
                                                     Size = a.Size,
                                                     Status = a.Status,
                                                     Error = a.Error,
                                                     LastSync = a.LastSync
                                                 });
    }

    private async Task ShowLogAsync(Data item)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var logs = await db.JobResults.FromClusterName(ClusterName)
                                      .Where(a => a.JobId == item.JobId && a.LastSync == item.LastSync)
                                      .Select(a => a.Logs)
                                      .FirstOrDefaultAsync();

        await dialogService.OpenSideLogAsync(L["Log"], logs!);
    }

    private async Task RemoveAllDataAsync()
    {
        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Delete all data"], true))
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            await db.JobResults.DeleteAsync(ClusterName);
            await eventNotificationService.PublishAsync(new DataChangedNotification());
            notificationService.Info(L["All data has been removed!"]);
        }
    }

    private void Scan()
    {
        backgroundJobService.Enqueue<Job>(a => a.ScanAsync(ClusterName));
        notificationService.Info(L["Scan started!"]);
    }

    public void Dispose()
    {
        eventNotificationService.Unsubscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        _loader?.Dispose();
    }
}
