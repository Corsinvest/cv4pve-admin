using Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;

namespace Corsinvest.ProxmoxVE.Admin.Module.ReplicationAnalytics.Components.Widgets;

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
        var status = await db.JobResults.Where(a => ClusterNames.Contains(a.ClusterName), ClusterNames.Any())
                                        .Where(a => a.Start > DateTime.UtcNow.AddDays(-60 * 3))
                                        .Select(a => new
                                        {
                                            a.Status,
                                            a.Start
                                        })
                                        .ToListAsync();

        SetOk(status.Count(a => a.Status));
        SetKo(status.Count(a => !a.Status));

        LastExecution = status.OrderByDescending(a => a.Start).FirstOrDefault()?.Start;
        MakeFooterText($"{L["Last"]}: {(LastExecution.HasValue ? LastExecution.Value.ToLocalTime().ToString("g") : string.Empty)}");
    }

    public override void Dispose()
    {
        eventNotificationService.Unsubscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        base.Dispose();
    }
}
