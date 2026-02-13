/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Components;

public partial class Status(IAdminService adminService,
                            IBackgroundJobService backgroundJobService,
                            ISettingsService settingsService,
                            ILoggerFactory loggerFactory,
                            DialogService dialogService,
                            IDbContextFactory<ModuleDbContext> dbContextFactory,
                            EventNotificationService eventNotificationService,
                            NotificationService notificationService) : IRefreshableData,
                                                                       IClusterName,
                                                                       IDisposable
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;
    [Parameter] public string VmIds { get; set; } = default!;

    private IEnumerable<AutoSnapInfo> Items { get; set; } = default!;
    private bool IsLoading { get; set; }
    private IList<AutoSnapInfo> SelectedItems { get; set; } = [];
    private bool AllowCalculateSnapshotSize { get; set; }
    private bool IsCalculateSnapshotSize { get; set; }

    protected override async Task OnInitializedAsync()
    {
        eventNotificationService.Subscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        await RefreshDataAsync();
    }

    private async Task HandleDataChangedNotificationAsync(DataChangedNotification notification) => await RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        if (IsLoading) { return; }

        IsLoading = true;
        await InvokeAsync(StateHasChanged);

        var settings = settingsService.GetForModule<Module, Settings>(ClusterName);

        var vmIds = VmIds;
        if (string.IsNullOrWhiteSpace(vmIds))
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            vmIds = settings.SearchMode == SearchMode.Managed
                        ? (await db.Jobs.FromClusterName(ClusterName)
                                        .Select(a => a.VmIds)
                                        .ToListAsync()).JoinAsString(",")
                        : ActionHelper.AllVms;
        }

        var clusterClient = adminService[ClusterName];
        AllowCalculateSnapshotSize = clusterClient.Settings.AllowCalculateSnapshotSize;

        var data = await ActionHelper.GetInfoAsync(await clusterClient.GetPveClientAsync(), settings, loggerFactory, vmIds);

        Items = [.. data];
        SelectedItems.Clear();

        IsLoading = false;

        if (AllowCalculateSnapshotSize)
        {
            IsCalculateSnapshotSize = true;
            await InvokeAsync(StateHasChanged);

            //snapshot size
            var disks = await clusterClient.CachedData.GetDisksInfoAsync(false);
            foreach (var item in Items)
            {
                item.SnapshotsSize = DiskInfoHelper.CalculateSnapshots(item.VmId, item.Name, disks);
            }

            IsCalculateSnapshotSize = false;
        }

        await InvokeAsync(StateHasChanged);
    }

    private void OnRender(DataGridRenderEventArgs<AutoSnapInfo> args)
    {
        if (args.FirstRender)
        {
            args.Grid!.Groups.Add(new()
            {
                Title = L["Label"],
                Property = nameof(AutoSnapInfo.Label)
            });

            args.Grid.Groups.Add(new()
            {
                Title = L["Vm Id"],
                Property = nameof(AutoSnapInfo.VmId)
            });

            StateHasChanged();
        }
    }

    private static void OnGroupRowRender(GroupRowRenderEventArgs args)
    {
        if (args.FirstRender)
        {
            args.Expanded = false;
        }
    }

    private async Task KeyDownAsync(KeyboardEventArgs e)
    {
        if (SelectedItems.Any() && e.IsForDelete()) { await DeleteAsync(); }
        else if (SelectedItems.Any() && e.IsForEdit()) { }
        else if (e.IsForNew()) { }
    }

    private async Task DeleteAsync()
    {
        if (!await HasPermissionAsync(ClusterName, Module.Permissions.Status.Delete)) { return; }

        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Delete selected rows"], true))
        {
            backgroundJobService.Schedule<Job>(a => a.DeleteAsync(SelectedItems, ClusterName), TimeSpan.FromSeconds(5));
            notificationService.Info(L["Deleting snapshots started!"]);
            SelectedItems.Clear();
        }
    }

    public void Dispose() => eventNotificationService.Unsubscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
}
