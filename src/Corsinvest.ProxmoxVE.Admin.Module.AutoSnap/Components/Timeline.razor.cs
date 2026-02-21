/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Components;

public partial class Timeline(IAdminService adminService,
                              EventNotificationService eventNotificationService,
                              IDbContextFactory<ModuleDbContext> dbContextFactory) : IClusterName,
                                                                                     IRefreshableData,
                                                                                     IDisposable
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private IEnumerable<Data> Items { get; set; } = default!;
    private bool AllowCalculateSnapshotSize { get; set; }
    private bool InRefresh { get; set; }
    private bool IsCalculateSnapshotSize { get; set; }

    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private bool _disposed;
    private IEnumerable<VmDiskInfo> _disks = [];

    private record Data(DateTime Date, bool Status, int Count) : ISnapshotsSize
    {
        public double SnapshotsSize { get; set; }
    }

    private record DataJob(int Id, string Label, bool Status, int Count) : ISnapshotsSize
    {
        public double SnapshotsSize { get; set; }
    }

    private record DataSnapshot(string SnapName,
                                DateTime Start,
                                DateTime? End,
                                bool Status) : ISnapshotsSize
    {
        public TimeSpan Duration
            => End.HasValue
                ? (End - Start).Value
                : TimeSpan.Zero;

        public double SnapshotsSize { get; set; }
    }

    protected override async Task OnInitializedAsync()
    {
        eventNotificationService.Subscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        await RefreshDataAsync();
    }

    private async Task HandleDataChangedNotificationAsync(DataChangedNotification notification) => await RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        if (_disposed || !await _refreshLock.WaitAsync(0)) { return; }

        InRefresh = true;

        try
        {
            var clusterClient = adminService[ClusterName];
            AllowCalculateSnapshotSize = clusterClient.Settings.AllowCalculateSnapshotSize;

            await using var db = await dbContextFactory.CreateDbContextAsync();

            Items = [.. db.Results
                          .Where(a => a.Job.ClusterName == ClusterName)
                          .GroupBy(a => a.Start.Date)
                          .OrderByDescending(a => a.Key)
                          .Select(a => new Data(a.Key, a.Any(b => b.Status), a.Count()))
                          .Take(10)];

            await InvokeAsync(StateHasChanged);

            if (AllowCalculateSnapshotSize)
            {
                IsCalculateSnapshotSize = true;

                //snapshot size
                _disks = await clusterClient.CachedData.GetDisksInfoAsync(false);

                foreach (var item in Items)
                {
                    var names = db.Results
                                  .Where(a => a.Job.ClusterName == ClusterName && a.Start.Date == item.Date)
                                  .Select(a => a.SnapName)
                                  .ToList();

                    item.SnapshotsSize = names.Select(a => DiskInfoHelper.CalculateSnapshots(0, a, _disks))
                                              .DefaultIfEmpty(0)
                                              .Sum();
                }

                IsCalculateSnapshotSize = false;
            }

            await InvokeAsync(StateHasChanged);
        }
        finally
        {
            InRefresh = false;
            if (!_disposed) { _refreshLock.Release(); }
        }
    }

    private string GetTitle(Data item)
    {
        var ret = item.Date.ToShortDateString();
        if (AllowCalculateSnapshotSize) { ret += $" - {FormatHelper.FromBytes(item.SnapshotsSize)}"; }
        return ret;
    }

    private string GetTitle(DataJob job)
    {
        var ret = job.Label;
        if (AllowCalculateSnapshotSize) { ret += $" - {FormatHelper.FromBytes(job.SnapshotsSize)}"; }
        return ret;
    }

    private List<DataJob> GetJobs(DateTime start)
    {
        using var db = dbContextFactory.CreateDbContext();
        var items = db.Jobs
                      .FromClusterName(ClusterName)
                      .Where(a => a.Results.Any(b => b.Start.Date == start))
                      .Select(a => new DataJob(a.Id,
                                               a.Label,
                                               a.Results.Any(b => b.Start.Date == start && b.Status),
                                               a.Results.Count(b => b.Start.Date == start)))
                      .ToList();

        foreach (var item in items)
        {
            var names = db.Results
                          .Where(a => a.Job.Id == item.Id && a.Start.Date == start)
                          .Select(a => a.SnapName)
                          .ToList();

            item.SnapshotsSize = names.Select(a => DiskInfoHelper.CalculateSnapshots(0, a, _disks))
                                      .DefaultIfEmpty(0)
                                      .Sum();
        }

        return items;
    }

    private List<DataSnapshot> Get(Data data, DataJob job)
    {
        using var db = dbContextFactory.CreateDbContext();
        var items = db.Results.Where(a => a.Job.Id == job.Id && a.Start.Date == data.Date)
                              .Select(a => new DataSnapshot(a.SnapName, a.Start, a.End, a.Status))
                              .ToList();

        foreach (var item in items)
        {
            item.SnapshotsSize = DiskInfoHelper.CalculateSnapshots(0, item.SnapName, _disks);
        }

        return items;
    }

    public void Dispose()
    {
        _disposed = true;
        eventNotificationService.Unsubscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        _refreshLock.Dispose();
    }
}
