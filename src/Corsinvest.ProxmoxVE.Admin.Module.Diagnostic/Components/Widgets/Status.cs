using Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;

namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Components.Widgets;

public class Status(IDbContextFactory<ModuleDbContext> dbContextFactory,
                    EventNotificationService eventNotificationService) : WidgetDonutBase<object>
{
    protected override async Task OnInitializedAsync()
    {
        SerieTitle = L["Count"];
        eventNotificationService.Subscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        await base.OnInitializedAsync();
    }

    private async Task HandleDataChangedNotificationAsync(DataChangedNotification notification)
    {
        await RefreshDataAsyncInt();
        await InvokeAsync(StateHasChanged);
    }

    protected override async Task RefreshDataAsyncInt()
    {
        Clear();

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var results = await db.JobResults.Where(a => ClusterNames.Contains(a.ClusterName), ClusterNames.Any())
                                         .ToListAsync();

        var totalInfo = 0;
        var totalWarning = 0;
        var totalCritical = 0;

        foreach (var item in results.GroupBy(a => a.ClusterName))
        {
            var data = item.OrderByDescending(a => a.Start).FirstOrDefault()!;
            totalInfo += data.Info;
            totalWarning += data.Warning;
            totalCritical += data.Critical;
        }

        Set("Info", totalInfo, Colors.Info);
        Set("Warning", totalWarning, Colors.Warning);
        Set("Critical", totalCritical, "var(--rz-error)");

        LastExecution = results.OrderByDescending(a => a.Start).FirstOrDefault()?.Start;
        MakeFooterText($"{L["Last"]}: {(LastExecution.HasValue ? LastExecution.Value.ToLocalTime().ToString("g") : string.Empty)}");
    }

    public override void Dispose()
    {
        eventNotificationService.Unsubscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        base.Dispose();
    }
}
