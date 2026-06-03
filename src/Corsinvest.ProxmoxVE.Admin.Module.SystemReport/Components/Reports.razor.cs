/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Services;
using Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Persistence;
using Corsinvest.ProxmoxVE.Report;

namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Components;

public partial class Reports(IBrowserService browserService,
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
    private bool _validColumnClick;
    private GridLoader<JobResult, Data>? _loader;

    private record Data(int Id,
                        Report.Settings Settings,
                        ReportFormat Format,
                        DateTime Start,
                        DateTime? End)
    {
        public TimeSpan Duration => End.HasValue ? (End - Start).Value : TimeSpan.Zero;
    }

    protected override async Task OnInitializedAsync()
    {
        eventNotificationService.Subscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        await RefreshDataAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            _loader = GridLoader.Create<JobResult, Data>(DataGridRef, defaultOrderBy: "Start desc, Id desc");
        }
    }

    private async Task HandleDataChangedNotificationAsync(DataChangedNotification notification)
    {
        await RefreshDataAsync();
        await InvokeAsync(StateHasChanged);
    }

    private static bool ShowWating(Data item) => item.Start == DateTime.MinValue && item.End == null;

    private async Task LoadDataAsync(LoadDataArgs args)
    {
        if (_loader is null) { return; }
        await using var db = await dbContextFactory.CreateDbContextAsync();
        ResultLoadData = await _loader.LoadAsync(db.JobResults.FromClusterName(ClusterName),
                                                 args,
                                                 a => new Data(a.Id,
                                                               a.Settings,
                                                               a.Format,
                                                               a.Start,
                                                               a.End));
    }

    public Task RefreshDataAsync()
        => _loader?.RefreshAsync() ?? (DataGridRef != null ? InvokeAsync(DataGridRef.Reload) : Task.CompletedTask);

    private void CellClick(DataGridCellMouseEventArgs<Data> e)
        => _validColumnClick = e.Column!.Property == nameof(Data.Id);

    private async Task RowSelectAsync(Data item)
    {
        if (_validColumnClick)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            await ShowEditorAsync((await db.JobResults.FromIdAsync(item.Id))!, EditDialogMode.ReadOnly);
        }
    }

    private Task<dynamic?> ShowEditorAsync(JobResult item, EditDialogMode mode)
        => dialogService.OpenSideEditAsync<ReportDialog>(item.Id == 0
                                                            ? L["New Report"]
                                                            : L["Report {0}", item.Id],
                                                         mode,
                                                         item);

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
        try
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            var job = (await db.JobResults.FromIdAsync(SelectedItems[0].Id))!;

            if (!File.Exists(job.FileName))
            {
                notificationService.Notify(NotificationSeverity.Warning, L["File not available for this report."]);
                return;
            }

            await using var fileStream = new FileStream(job.FileName, FileMode.Open, FileAccess.Read);
            await browserService.DownloadFileAsync($"SystemReport-{job.ClusterName}-{job.Start:yyyyMMdd-HHmmss}.zip",
                                                   fileStream,
                                                   "application/zip");
        }
        finally
        {
            InDownload = false;
        }
    }

    private async Task AddAsync()
    {
        var item = new JobResult
        {
            ClusterName = ClusterName,
            Logs = string.Empty,
        };

        if (await ShowEditorAsync(item, EditDialogMode.Create) != null)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            await db.JobResults.AddAsync(item);
            await db.SaveChangesAsync();

            backgroundJobService.Enqueue<Job>(a => a.GenerateAsync(item.Id));
            notificationService.Info(L["Report started!"]);

            await DataGridRef.Reload();
        }
    }

    public void Dispose()
    {
        eventNotificationService.Unsubscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        _loader?.Dispose();
    }
}
