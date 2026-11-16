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

    private const int DaysPage = 10;

    private IEnumerable<Data> Items { get; set; } = [];
    private bool AllowCalculateSnapshotSize { get; set; }
    private bool InRefresh { get; set; }
    private bool IsCalculateSnapshotSize { get; set; }
    private bool HasMore { get; set; }

    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly HashSet<DateTime> _expanded = [];
    private bool _disposed;
    private int _days = DaysPage;
    private List<int>? _visibleJobIds;

    private record Slot(int Ok, int Failed);

    private record Data(DateTime Date, List<DataSnapshot> Snapshots)
    {
        public int Count => Snapshots.Count;
        public int Failed => Snapshots.Count(a => !a.Status);
        public double SnapshotsSize => Snapshots.Sum(a => a.SnapshotsSize);

        public Slot[] Hours { get; } = [.. Enumerable.Range(0, 24)
                                                     .Select(hour => Snapshots.Where(a => a.Start.ToLocalTime().Hour == hour).ToList())
                                                     .Select(a => new Slot(a.Count(b => b.Status), a.Count(b => !b.Status)))];
    }

    private record DataSnapshot(string Job,
                                string SnapName,
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

    private Task HandleDataChangedNotificationAsync(DataChangedNotification notification) => RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        if (_disposed || !await _refreshLock.WaitAsync(0)) { return; }

        InRefresh = true;

        try
        {
            var clusterClient = adminService[ClusterName];
            AllowCalculateSnapshotSize = clusterClient.Settings.AllowCalculateSnapshotSize;

            await using var db = await dbContextFactory.CreateDbContextAsync();
            _visibleJobIds = await ActionHelper.GetVisibleJobIdsAsync(PermissionService, db, ClusterName);

            // The size of a snapshot name is summed over every VM of the cluster: shown only to who can read them all
            AllowCalculateSnapshotSize = AllowCalculateSnapshotSize && _visibleJobIds == null;

            var results = db.Results
                            .Where(a => a.Job.ClusterName == ClusterName)
                            .Where(a => _visibleJobIds!.Contains(a.Job.Id), _visibleJobIds != null);

            // Days are counted in UTC by the database, then regrouped in local time: one more day covers the offset
            var utcDays = results.Select(a => a.Start.Date)
                                 .Distinct()
                                 .OrderByDescending(a => a)
                                 .Take(_days + 2)
                                 .ToList();

            var days = new List<Data>();
            if (utcDays.Count > 0)
            {
                var from = DateTime.SpecifyKind(utcDays.Min().AddDays(-1), DateTimeKind.Utc);
                days = [.. results.Where(a => a.Start >= from)
                                  .Select(a => new DataSnapshot(a.Job.Label, a.SnapName, a.Start, a.End, a.Status))
                                  .ToList()
                                  .GroupBy(a => a.Start.ToLocalTime().Date)
                                  .OrderByDescending(a => a.Key)
                                  .Select(a => new Data(a.Key, [.. a]))];
            }

            HasMore = days.Count > _days;
            Items = [.. days.Take(_days)];

            await InvokeAsync(StateHasChanged);

            if (AllowCalculateSnapshotSize)
            {
                IsCalculateSnapshotSize = true;
                await InvokeAsync(StateHasChanged);

                var disks = await clusterClient.CachedData.GetStorageSnapshotInfosAsync(false);
                foreach (var snapshot in Items.SelectMany(a => a.Snapshots))
                {
                    snapshot.SnapshotsSize = StorageSnapshotHelper.CalculateSnapshots(0, snapshot.SnapName, disks);
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

    private static PointStyle GetPointStyle(Data item)
        => item.Failed == 0
            ? PointStyle.Success
            : item.Failed == item.Count
                ? PointStyle.Danger
                : PointStyle.Warning;

    private static string GetSlotColor(Slot slot)
        => (slot.Ok, slot.Failed) switch
        {
            (0, 0) => "var(--rz-base-200)",
            (_, 0) => "var(--rz-success)",
            (0, _) => "var(--rz-danger)",
            _ => "var(--rz-warning)"
        };

    private string GetSlotTitle(int hour, Slot slot)
    {
        var ret = $"{hour:00}:00";
        if (slot.Ok > 0) { ret += $" · {L["{0} snapshots", slot.Ok]}"; }
        if (slot.Failed > 0) { ret += $" · {L["{0} failed", slot.Failed]}"; }
        return ret;
    }

    private string? GetRelativeDay(DateTime date)
        => date == DateTime.Today
            ? L["Today"].Value
            : date == DateTime.Today.AddDays(-1)
                ? L["Yesterday"].Value
                : null;

    private void Toggle(DateTime date)
    {
        if (!_expanded.Remove(date)) { _expanded.Add(date); }
    }

    private async Task ShowMoreAsync()
    {
        _days += DaysPage;
        await RefreshDataAsync();
    }

    public void Dispose()
    {
        _disposed = true;
        eventNotificationService.Unsubscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        _refreshLock.Dispose();
    }
}
