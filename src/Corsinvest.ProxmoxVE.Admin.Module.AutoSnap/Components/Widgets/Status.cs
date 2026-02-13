/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;

namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Components.Widgets;

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
        var results = db.Jobs.Where(a => ClusterNames.Contains(a.ClusterName), ClusterNames.Any())
                             .SelectMany(a => a.Results)
                             .Select(a => new
                             {
                                 a.Status,
                                 a.Start
                             });

        SetOk(results.Count(a => a.Status));
        SetKo(results.Count(a => !a.Status));

        LastExecution = (await results.OrderByDescending(a => a.Start).FirstOrDefaultAsync())?.Start;
        MakeFooterText($"{L["Last"]}: {(LastExecution.HasValue ? LastExecution.Value.ToLocalTime().ToString("g") : string.Empty)}");
    }

    public override void Dispose()
    {
        eventNotificationService.Unsubscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        base.Dispose();
    }
}
