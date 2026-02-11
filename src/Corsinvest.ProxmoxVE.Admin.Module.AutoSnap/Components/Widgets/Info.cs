using Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;

namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Components.Widgets;

public class Info(IAdminService adminService,
                  ISettingsService settingsService,
                  ILoggerFactory loggerFactory,
                  IDbContextFactory<ModuleDbContext> dbContextFactory,
                  EventNotificationService eventNotificationService) : WidgetInfoBase<object>
{
    protected override async Task OnInitializedAsync()
    {
        eventNotificationService.Subscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        await base.OnInitializedAsync();
    }

    private async Task HandleDataChangedNotificationAsync(DataChangedNotification notification)
    {
        await RefreshDataAsync();
        await InvokeAsync(StateHasChanged);
    }

    protected override async Task RefreshDataAsyncInt()
    {
        // Reset loading states
        IsLoadingStats = true;
        IsLoadingAlerts = true;
        IsLoadingDistribution = true;
        Alerts = [];

        // Initialize Stats and Distribution with zero values
        UpdateStatsAndDistribution([], 0);
        await InvokeAsync(StateHasChanged);

        var clusterNames = ClusterNames.Any()
                  ? ClusterNames
                  : settingsService.GetEnabledClustersSettings().Select(a => a.Name);

        var allSnapshots = new List<AutoSnapInfo>();
        var totalJobs = 0;

        await using var db = await dbContextFactory.CreateDbContextAsync();

        // Load snapshots from all clusters
        foreach (var clusterName in clusterNames)
        {
            var clusterClient = adminService[clusterName];
            var settings = settingsService.GetForModule<Module, Settings>(clusterName);

            var data = await ActionHelper.GetInfoAsync(await clusterClient.GetPveClientAsync(),
                                                       settings,
                                                       loggerFactory,
                                                       ActionHelper.AllVms);

            allSnapshots.AddRange(data);

            // Count active jobs
            totalJobs += await db.Jobs.FromClusterName(clusterName).CountAsync(a => a.Enabled);

            // Update Stats incrementally
            UpdateStatsAndDistribution(allSnapshots, totalJobs);
            await InvokeAsync(StateHasChanged);
        }

        IsLoadingStats = false;
        IsLoadingDistribution = false;

        // Build alerts
        var alerts = new List<AlertItem>();
        var now = DateTime.UtcNow;
        var moreThan90d = allSnapshots.Count(a => (now - a.Date).TotalDays >= 90);
        var vmsWithSnapshots = allSnapshots.Select(a => a.VmId).Distinct().Count();
        var stoppedVmsWithSnapshots = allSnapshots.Where(a => !a.VmStatus).Select(a => a.VmId).Distinct().Count();

        // Check for failed snapshots in last 7 days (only for selected clusters)
        var failedLast7Days = await db.Results
                                      .Where(a => clusterNames.Contains(a.Job.ClusterName))
                                      .Where(a => !a.Status && a.Start > DateTime.UtcNow.AddDays(-7))
                                      .CountAsync();

        if (failedLast7Days > 0) { alerts.Add(new AlertItem("error", "rz-color-danger", L["{0} failed snapshots in last 7 days", failedLast7Days])); }
        if (stoppedVmsWithSnapshots > 0) { alerts.Add(new AlertItem("power_settings_new", "rz-color-warning", L["{0} stopped VMs with snapshots", stoppedVmsWithSnapshots])); }
        if (moreThan90d > 0) { alerts.Add(new AlertItem("event_busy", "rz-color-base-500", L["{0} snapshots older than 90 days", moreThan90d])); }

        Alerts = alerts;
        IsLoadingAlerts = false;
        await InvokeAsync(StateHasChanged);
    }

    private void UpdateStatsAndDistribution(List<AutoSnapInfo> snapshots, int totalJobs)
    {
        var count = snapshots.Count;
        var vmsWithSnapshots = snapshots.Select(a => a.VmId).Distinct().Count();

        Stats =
        [
            new (L["TOTAL"], count.ToString(), L["snapshots"], Colors.Primary),
            new (L["VMS"], vmsWithSnapshots.ToString(), L["snapshots"], Colors.Success),
            new (L["JOBS"], totalJobs.ToString(), L["active jobs"], Colors.Info),
            new (L["FAILED"], "...", L["last 7 days"], Colors.Danger)
        ];

        var now = DateTime.UtcNow;
        var lessThan7d = snapshots.Count(a => (now - a.Date).TotalDays < 7);
        var between7And30d = snapshots.Count(a => (now - a.Date).TotalDays >= 7 && (now - a.Date).TotalDays < 30);
        var between30And90d = snapshots.Count(a => (now - a.Date).TotalDays >= 30 && (now - a.Date).TotalDays < 90);
        var moreThan90d = snapshots.Count(a => (now - a.Date).TotalDays >= 90);
        var maxAge = Math.Max(1, Math.Max(lessThan7d, Math.Max(between7And30d, Math.Max(between30And90d, moreThan90d))));

        AgeDistribution =
        [
            new ("< 7d", lessThan7d, lessThan7d * 100 / maxAge, ProgressBarStyle.Success),
            new ("7-30d", between7And30d, between7And30d * 100 / maxAge, ProgressBarStyle.Info),
            new ("30-90d", between30And90d, between30And90d * 100 / maxAge, ProgressBarStyle.Warning),
            new ("> 90d", moreThan90d, moreThan90d * 100 / maxAge, ProgressBarStyle.Danger)
        ];
    }

    public override void Dispose()
    {
        eventNotificationService.Unsubscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        base.Dispose();
    }
}
