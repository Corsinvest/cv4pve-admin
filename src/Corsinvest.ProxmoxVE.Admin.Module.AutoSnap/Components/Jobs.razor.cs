/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Components;

public partial class Jobs(IDbContextFactory<ModuleDbContext> dbContextFactory,
                          IBackgroundJobService backgroundJobService,
                          DialogService dialogService,
                          NotificationService notificationService,
                          EventNotificationService eventNotificationService) : IRefreshableData,
                                                                               IClusterName,
                                                                               IDisposable
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private RadzenDataGrid<Data> DataGridRef { get; set; } = default!;
    private ResultLoadData<Data> ResultLoadData { get; set; } = new(null!, -1, null); private IList<Data> SelectedItems { get; set; } = [];

    private bool _validColumnClick;

    private class Data : JobSchedule;

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
        if (DataGridRef != null) { await InvokeAsync(DataGridRef.Reload); }
    }

    private async Task LoadDataAsync(LoadDataArgs args)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        ResultLoadData = await DataGridRef.LoadDataAsync(db.Jobs.FromClusterName(ClusterName),
                                                         args,
                                                         a => new Data
                                                         {
                                                             Id = a.Id,
                                                             Enabled = a.Enabled,
                                                             VmIdsList = a.VmIdsList,
                                                             Label = a.Label,
                                                             Description = a.Description,
                                                             Keep = a.Keep,
                                                             VmStatus = a.VmStatus,
                                                             OnlyRuns = a.OnlyRuns,
                                                             TimeoutSnapshot = a.TimeoutSnapshot
                                                         },
                                                         ResultLoadData.Filter);
    }

    private async Task DeleteAsync()
    {
        if (!await HasPermissionAsync(ClusterName, Module.Permissions.Job.Data.Delete)) { return; }

        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Delete selected row"], true))
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            await db.Jobs.DeleteAsync(SelectedItems[0].Id);

            backgroundJobService.Schedule<Job>(a => a.DeleteAsync(SelectedItems[0].Id), TimeSpan.FromSeconds(5));
            notificationService.Info(L["Delete started!"]);

            SelectedItems.Clear();
        }
    }

    private async Task KeyDownAsync(KeyboardEventArgs e)
    {
        if (SelectedItems.Any() && e.IsForDelete()) { await DeleteAsync(); }
        else if (SelectedItems.Any() && e.IsForEdit()) { await RowSelectAsync(SelectedItems[0]); }
        else if (e.IsForNew()) { await AddAsync(); }
    }

    private async Task RowSelectAsync(Data item)
    {
        if (_validColumnClick)
        {
            if (!await HasPermissionAsync(ClusterName, Module.Permissions.Job.Data.Edit)) { return; }
            await using var db = await dbContextFactory.CreateDbContextAsync();
            await ShowEditorAsync((await db.Jobs.FromIdAsync(item.Id))!);
        }
    }

    private void CellClick(DataGridCellMouseEventArgs<Data> e)
        => _validColumnClick = new[] { nameof(Data.Id), nameof(Data.Label) }.Contains(e.Column!.Property);

    private async Task PurgeAsync()
    {
        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Clean selected row"], false))
        {
            backgroundJobService.Schedule<Job>(a => a.PurgeAsync(SelectedItems[0].Id), TimeSpan.FromSeconds(5));
            notificationService.Info(L["Purge started!"]);
        }
    }

    private void Snap()
    {
        backgroundJobService.Schedule<Job>(a => a.SnapAsync(SelectedItems[0].Id), TimeSpan.FromSeconds(5));
        notificationService.Info(L["Job started!"]);
    }

    private async Task AddAsync()
    {
        if (!await HasPermissionAsync(ClusterName, Module.Permissions.Job.Data.Create)) { return; }
        await ShowEditorAsync(new() { ClusterName = ClusterName });
    }

    private async Task ShowEditorAsync(JobSchedule item)
    {
        var isNew = item.Id == 0;
        var title = isNew
                        ? L["New"]
                        : L["Edit {0}", item.Id];

        if (await dialogService.OpenSideEditAsync<JobDialog>(title, isNew, item) != null)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            await db.AddOrUpdateAsync(item);

            backgroundJobService.ScheduleOrRemove<Job>(a => a.SnapAsync(item.Id),
                                             item.CronExpression,
                                             item.Enabled,
                                             item.ClusterName,
                                             item.Id.ToString());

            await DataGridRef.Reload();
        }
    }

    public void Dispose() => eventNotificationService.Unsubscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
}
