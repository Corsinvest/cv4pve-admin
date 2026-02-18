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

    private class Data : JobResult;

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
        if (DataGridRef != null) { await InvokeAsync(DataGridRef.Reload); }
    }

    private async Task LoadDataAsync(LoadDataArgs args)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        ResultLoadData = await DataGridRef.LoadDataAsync(db.JobResults.FromClusterName(ClusterName),
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
                                                         },
                                                         ResultLoadData.Filter);
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
        backgroundJobService.Schedule<Job>(a => a.ScanFromResultAsync(ClusterName), TimeSpan.FromSeconds(5));
        notificationService.Info(L["Scan started!"]);
    }

    public void Dispose() => eventNotificationService.Unsubscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
}
