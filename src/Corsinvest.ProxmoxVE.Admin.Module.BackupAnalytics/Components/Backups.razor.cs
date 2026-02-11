using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Module.BackupAnalytics.Components;

public partial class Backups(IDbContextFactory<ModuleDbContext> dbContextFactory,
                             IBackgroundJobService backgroundJobService,
                             ISettingsService settingsService,
                             DialogService dialogService,
                             NotificationService notificationService,
                             IAdminService adminService,
                             EventNotificationService eventNotificationService) : IRefreshableData,
                                                                                  IClusterName,
                                                                                  IDisposable
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private RadzenDataGrid<Data> DataGridRef { get; set; } = default!;
    private ResultLoadData<Data> ResultLoadData { get; set; } = new(null!, -1, null);
    private Settings Settings { get; set; } = new();
    private IEnumerable<VmData> Vms { get; set; } = [];

    private record VmData(string VmId,
                          string Name,
                          string Description,
                          VmType VmType);

    private class Data(IEnumerable<VmData> vms) : JobResult
    {
        private VmData vm = null!;
        private VmData GetVm() => vm ??= vms.FirstOrDefault(a => a.VmId == VmId)!;

        public string TaskId { get; set; } = default!;
        public string Node { get; set; } = default!;
        public string Storage { get; set; } = default!;
        public string VmName => GetVm().Name;
        public string VmDescription => GetVm().Description;
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

    public async Task RefreshDataAsync()
    {
        Settings = settingsService.GetForModule<Module, Settings>(ClusterName);
        if (DataGridRef != null) { await InvokeAsync(DataGridRef.Reload); }
    }

    private async Task LoadDataAsync(LoadDataArgs args)
    {
        Vms = [.. (await adminService[ClusterName].CachedData.GetResourcesAsync(false))
                    .Where(a => a.ResourceType == ClusterResourceType.Vm)
                    .Select(a => new VmData(a.VmId.ToString(), a.Name, a.Description, a.VmType))];

        await using var db = await dbContextFactory.CreateDbContextAsync();
        ResultLoadData = await DataGridRef.LoadDataAsync(db.JobResults.Where(a => a.TaskResult.ClusterName == ClusterName),
                                                         args,
                                                         a => new Data(Vms)
                                                         {
                                                             TaskId = a.TaskResult.TaskId!,
                                                             Node = a.TaskResult.Node!,
                                                             Storage = a.TaskResult.Storage!,
                                                             Archive = a.Archive!,
                                                             Start = a.Start,
                                                             End = a.End,
                                                             Error = a.Error,
                                                             Size = a.Size,
                                                             Status = a.Status,
                                                             TransferSize = a.TransferSize,
                                                             VmId = a.VmId
                                                         },
                                                         ResultLoadData.Filter);
    }

    private async Task ShowLogAsync(Data item, bool forJob)
    {
        var title = forJob
                ? L["Full Task"] // oppure L["Full Task {0}", taskId]
                : L["Job Vm: {0}", item.VmId!];

        await using var db = await dbContextFactory.CreateDbContextAsync();

        var logs = forJob
                    ? await db.TaskResults.Where(a => a.TaskId == item.TaskId!)
                                          .Select(a => a.Logs)
                                          .FirstOrDefaultAsync()

                    : await db.JobResults.Where(a => a.VmId == (string?)item.VmId && a.TaskResult.TaskId == item.TaskId!)
                                         .Select(a => a.Logs)
                                         .FirstOrDefaultAsync();

        await dialogService.OpenSideLogAsync(title, logs!);
    }

    private static void RowRender(RowRenderEventArgs<Data> args)
    {
        if (!args.Data!.Status || !string.IsNullOrEmpty(args.Data.Error)) { args.SetRowStyleError(); }
    }

    private async Task RemoveAllDataAsync()
    {
        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Remove all data"], true))
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            await db.TaskResults.DeleteAsync(ClusterName);
            await eventNotificationService.PublishAsync(new DataChangedNotification());
            //            await DataGridRef.Reload();
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
