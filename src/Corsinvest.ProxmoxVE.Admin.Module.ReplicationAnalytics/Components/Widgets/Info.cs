using Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;

namespace Corsinvest.ProxmoxVE.Admin.Module.ReplicationAnalytics.Components.Widgets;

public class Info(IDbContextFactory<ModuleDbContext> dbContextFactory,
                  ISettingsService settingsService,
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
        IsLoadingStats = true;
        IsLoadingAlerts = true;
        IsLoadingDistribution = true;
        Alerts = [];

        UpdateStatsAndDistribution([]);
        await InvokeAsync(StateHasChanged);

        var clusterNames = ClusterNames.Any()
                  ? ClusterNames
                  : settingsService.GetEnabledClustersSettings().Select(a => a.Name);

        await using var db = await dbContextFactory.CreateDbContextAsync();

        // Get all replication jobs from last 90 days
        var replications = await db.JobResults
            .Where(a => ClusterNames.Contains(a.ClusterName), ClusterNames.Any())
            .Where(a => a.Start > DateTime.UtcNow.AddDays(-90))
            .ToListAsync();

        // Update Stats
        UpdateStatsAndDistribution(replications);
        IsLoadingStats = false;
        IsLoadingDistribution = false;
        await InvokeAsync(StateHasChanged);

        // Build alerts
        var count = replications.Count;
        var failed = replications.Count(a => !a.Status);
        var withErrors = replications.Count(a => !string.IsNullOrEmpty(a.Error));
        var now = DateTime.UtcNow;
        var moreThan7d = replications.Count(a => (now - a.LastSync).TotalDays >= 7);
        var moreThan24h = replications.Count(a => (now - a.LastSync).TotalHours >= 24 && (now - a.LastSync).TotalDays < 7);

        var alerts = new List<AlertItem>();
        if (failed > 0) { alerts.Add(new AlertItem("error", "rz-color-danger", L["{0} failed replications", failed])); }
        if (withErrors > 0) { alerts.Add(new AlertItem("warning", "rz-color-warning", L["{0} replications with errors", withErrors])); }
        if (moreThan7d > 0) { alerts.Add(new AlertItem("event_busy", "rz-color-danger", L["{0} replications not synced for 7+ days", moreThan7d])); }
        if (moreThan24h > 0) { alerts.Add(new AlertItem("schedule", "rz-color-warning", L["{0} replications not synced for 24+ hours", moreThan24h])); }

        Alerts = alerts;
        IsLoadingAlerts = false;
        await InvokeAsync(StateHasChanged);
    }

    private void UpdateStatsAndDistribution(List<Models.JobResult> replications)
    {
        var count = replications.Count;
        var success = replications.Count(a => a.Status);
        var failed = replications.Count(a => !a.Status);
        var lastSync = replications.OrderByDescending(a => a.LastSync).FirstOrDefault()?.LastSync;

        string lastSyncText;
        string lastSyncSubtitle;

        if (lastSync.HasValue)
        {
            var timeAgo = DateTime.UtcNow - lastSync.Value;
            if (timeAgo.TotalHours < 1)
            {
                lastSyncText = $"{(int)timeAgo.TotalMinutes}m";
                lastSyncSubtitle = L["ago"];
            }
            else if (timeAgo.TotalDays < 1)
            {
                lastSyncText = $"{(int)timeAgo.TotalHours}h";
                lastSyncSubtitle = L["ago"];
            }
            else if (timeAgo.TotalDays < 7)
            {
                lastSyncText = $"{(int)timeAgo.TotalDays}d";
                lastSyncSubtitle = L["ago"];
            }
            else
            {
                lastSyncText = lastSync.Value.ToLocalTime().ToString("dd/MM/yy");
                lastSyncSubtitle = lastSync.Value.ToLocalTime().ToString("HH:mm");
            }
        }
        else
        {
            lastSyncText = "N/A";
            lastSyncSubtitle = L["never"];
        }

        Stats =
        [
            new (L["TOTAL"], count.ToString(), L["replications"], Colors.Primary),
            new (L["SUCCESS"], success.ToString(), $"{(count == 0 ? 0 : (success * 100 / count))}%", Colors.Success),
            new (L["FAILED"], failed.ToString(), $"{(count == 0 ? 0 : (failed * 100 / count))}%", Colors.Danger),
            new (L["LAST SYNC"], lastSyncText, lastSyncSubtitle, Colors.Info)
        ];

        var now = DateTime.UtcNow;
        var lessThan1h = replications.Count(a => (now - a.LastSync).TotalHours < 1);
        var between1And24h = replications.Count(a => (now - a.LastSync).TotalHours >= 1 && (now - a.LastSync).TotalHours < 24);
        var between1And7d = replications.Count(a => (now - a.LastSync).TotalDays >= 1 && (now - a.LastSync).TotalDays < 7);
        var moreThan7d = replications.Count(a => (now - a.LastSync).TotalDays >= 7);

        var maxAge = Math.Max(1, Math.Max(lessThan1h, Math.Max(between1And24h, Math.Max(between1And7d, moreThan7d))));
        AgeDistribution =
        [
            new ("< 1h", lessThan1h, lessThan1h * 100 / maxAge, ProgressBarStyle.Success),
            new ("1-24h", between1And24h, between1And24h * 100 / maxAge, ProgressBarStyle.Info),
            new ("1-7d", between1And7d, between1And7d * 100 / maxAge, ProgressBarStyle.Warning),
            new ("> 7d", moreThan7d, moreThan7d * 100 / maxAge, ProgressBarStyle.Danger)
        ];
    }

    public override void Dispose()
    {
        eventNotificationService.Unsubscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        base.Dispose();
    }
}
