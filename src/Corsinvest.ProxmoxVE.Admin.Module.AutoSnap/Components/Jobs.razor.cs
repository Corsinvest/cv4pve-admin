/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Components;

public partial class Jobs(IDbContextFactory<ModuleDbContext> dbContextFactory,
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
    private ResultLoadData<Data> ResultLoadData { get; set; } = new(null!, -1, null); private IList<Data> SelectedItems { get; set; } = [];

    private bool _validColumnClick;
    private GridLoader<JobSchedule, Data>? _loader;

    private class Data : JobSchedule;

    protected override async Task OnInitializedAsync()
    {
        eventNotificationService.Subscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        await RefreshDataAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            _loader = GridLoader.Create<JobSchedule, Data>(DataGridRef, defaultOrderBy: "Label");
        }
    }

    private async Task HandleDataChangedNotificationAsync(DataChangedNotification notification)
    {
        await InvokeAsync(async () =>
        {
            await RefreshDataAsync();
            StateHasChanged();
        });
    }

    public Task RefreshDataAsync()
        => _loader?.RefreshAsync() ?? (DataGridRef != null ? InvokeAsync(DataGridRef.Reload) : Task.CompletedTask);

    private async Task LoadDataAsync(LoadDataArgs args)
    {
        if (_loader is null) { return; }
        await using var db = await dbContextFactory.CreateDbContextAsync();
        ResultLoadData = await _loader.LoadAsync(await VisibleJobsAsync(db),
                                                 args,
                                                 a => new Data
                                                 {
                                                     Id = a.Id,
                                                     Enabled = a.Enabled,
                                                     VmIds = a.VmIds,
                                                     Label = a.Label,
                                                     Description = a.Description,
                                                     Keep = a.Keep,
                                                     VmStatus = a.VmStatus,
                                                     OnlyRuns = a.OnlyRuns,
                                                     TimeoutSnapshot = a.TimeoutSnapshot,
                                                     MaxParallel = a.MaxParallel,
                                                     CronExpression = a.CronExpression,
                                                     Results = a.Results.OrderByDescending(r => r.Start).Take(1).ToList()
                                                 });
    }

    private async Task<IQueryable<JobSchedule>> VisibleJobsAsync(ModuleDbContext db)
    {
        var visible = await ActionHelper.GetVisibleJobIdsAsync(PermissionService, db, ClusterName);
        return db.Jobs.FromClusterName(ClusterName).Where(a => visible!.Contains(a.Id), visible != null);
    }

    // Snapshots are taken, removed or purged on every VM of the job: the user must see and be allowed on all of them
    private async Task<bool> CanSnapshotAsync(params string[] vmIds)
    {
        var allowed = true;
        foreach (var item in vmIds)
        {
            allowed = allowed
                      && await PermissionService.CanReadVmSelectionAsync(ClusterName, item)
                      && await PermissionService.CanActOnVmSelectionAsync(ClusterName, Core.Security.Auth.ClusterPermissions.Vm.Snapshot, item);
        }
        if (allowed) { return true; }

        notificationService.Error(L["Not allowed on all the VMs of the job: snapshots need the Snapshot permission on each of them, and @all, pools and tags need Read on the whole cluster."]);
        return false;
    }

    private async Task DeleteAsync()
    {
        if (!await HasPermissionAsync(ClusterName, Module.Permissions.Job.Data.Delete)) { return; }
        if (!await CanSnapshotAsync(SelectedItems[0].VmIds)) { return; }

        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Delete selected row"], true))
        {
            backgroundJobService.Enqueue<Job>(a => a.DeleteAsync(SelectedItems[0].Id));
            notificationService.Info(L["Delete started!"]);

            SelectedItems.Clear();
        }
    }

    private Task KeyDownAsync(KeyboardEventArgs e) => e.HandleGridKeysAsync(SelectedItems.Any(), DeleteAsync, EditSelectedAsync, AddAsync);

    // The check on the clicked column is for the mouse: the key opens the editor of the selected row
    private Task EditSelectedAsync()
    {
        _validColumnClick = true;
        return RowSelectAsync(SelectedItems[0]);
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

    private Task ExportAsync()
        => DataGridRef.ExportToExcelAsync($"autosnap-jobs-{ClusterName}.xlsx",
                                          new()
                                          {
                                              Scope = DataGridExportScope.All,
                                              Title = L["Jobs"]
                                          });

    private async Task PurgeAsync()
    {
        if (!await HasPermissionAsync(ClusterName, Module.Permissions.Job.Purge)) { return; }
        if (!await CanSnapshotAsync(SelectedItems[0].VmIds)) { return; }

        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Clean selected row"], false))
        {
            backgroundJobService.Enqueue<Job>(a => a.PurgeAsync(SelectedItems[0].Id));
            notificationService.Info(L["Purge started!"]);
        }
    }

    private async Task SnapAsync()
    {
        if (!await HasPermissionAsync(ClusterName, Module.Permissions.Job.Snap)) { return; }
        if (!await CanSnapshotAsync(SelectedItems[0].VmIds)) { return; }

        backgroundJobService.Enqueue<Job>(a => a.SnapAsync(SelectedItems[0].Id));
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
        var previousVmIds = item.VmIds;
        if (await dialogService.OpenSideEditAsync<JobDialog>(isNew
                                                                ? L["New"]
                                                                : L["Edit {0}", item.Id],
                                                             isNew
                                                                ? EditDialogMode.Create
                                                                : EditDialogMode.Edit,
                                                             item) != null)
        {
            // A change must not drop VMs the user is not allowed on, nor add any
            if (!await CanSnapshotAsync(isNew ? [item.VmIds] : [item.VmIds, previousVmIds])) { return; }

            await using var db = await dbContextFactory.CreateDbContextAsync();
            await db.AddOrUpdateAsync(item);

            backgroundJobService.ScheduleOrRemove<Job>(a => a.SnapAsync(item.Id),
                                             item.CronExpression,
                                             item.Enabled && settingsService.GetForModule<Module, Settings>(item.ClusterName).Enabled,
                                             item.ClusterName,
                                             item.Id.ToString());

            await DataGridRef.Reload();
        }
    }

    public void Dispose()
    {
        eventNotificationService.Unsubscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        _loader?.Dispose();
    }
}
