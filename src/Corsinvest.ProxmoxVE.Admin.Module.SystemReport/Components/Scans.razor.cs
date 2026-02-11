using BlazorDownloadFile;
using Corsinvest.ProxmoxVE.Admin.Core.Services;
using Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Persistence;

namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Components;

public partial class Scans(IBlazorDownloadFileService blazorDownloadFileService,
                             IDbContextFactory<ModuleDbContext> dbContextFactory,
                             IBackgroundJobService backgroundJobService,
                             DialogService dialogService,
                             NotificationService notificationService,
                             EventNotificationService eventNotificationService) : IClusterName,
                                                                                  IRefreshableData,
                                                                                  IDisposable
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private IList<Data> SelectedItems { get; set; } = [];
    private bool InDownload { get; set; }
    private RadzenDataGrid<Data> DataGridRef { get; set; } = default!;
    private ResultLoadData<Data> ResultLoadData { get; set; } = new(null!, -1, null);

    private record Data(int Id,
                        string NodeNames,
                        NodeFeature NodeFeatures,
                        string VmIds,
                        VmFeature VmFeatures,
                        string StorageNames,
                        StorageFeature StorageFeatures,
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

    private static bool ShowWating(Data item) => item.Start == DateTime.MinValue && item.End == null;

    private async Task LoadDataAsync(LoadDataArgs args)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        ResultLoadData = await DataGridRef.LoadDataAsync(db.JobResults.FromClusterName(ClusterName),
                                                         args,
                                                         a => new Data(a.Id,
                                                                       a.NodeNames,
                                                                       a.NodeFeatures,
                                                                       a.VmIds,
                                                                       a.VmFeatures,
                                                                       a.StorageNames,
                                                                       a.StorageFeatures,
                                                                       a.Start,
                                                                       a.End),
                                                         ResultLoadData.Filter);
    }

    public async Task RefreshDataAsync()
    {
        if (DataGridRef != null) { await InvokeAsync(DataGridRef.Reload); }
    }

    private async Task DeleteAsync()
    {
        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Delete selected row"], true))
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            var job = (await db.JobResults.FromIdAsync(SelectedItems[0].Id))!;
            if (File.Exists(job.FileName)) { File.Delete(job.FileName); }

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
        var job = (await db.JobResults.FromIdAsync(SelectedItems[0].Id))!;
        await using var fileStream = new FileStream(job.FileName, FileMode.Open, FileAccess.Read);
        await blazorDownloadFileService.DownloadFile($"SystemReport-{job.ClusterName}-{job.Start}.xlsx",
                                                     fileStream,
                                                     "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        InDownload = false;
    }

    private async Task AddAsync()
    {
        var item = new JobResult
        {
            ClusterName = ClusterName,
            Logs = string.Empty
        };

        if (await dialogService.OpenSideEditAsync<ScanDialog>(L["New Scan"], true, item) != null)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            await db.JobResults.AddAsync(item);
            await db.SaveChangesAsync();

            backgroundJobService.Schedule<Job>(a => a.ScanAsync(item.Id), TimeSpan.FromSeconds(5));
            notificationService.Info(L["Scan started!"]);

            await DataGridRef.Reload();
        }
    }

    public void Dispose() => eventNotificationService.Unsubscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
}
