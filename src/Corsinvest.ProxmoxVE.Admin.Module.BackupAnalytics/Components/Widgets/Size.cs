/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Module.BackupAnalytics.Components.Widgets;

public class Size(IDbContextFactory<ModuleDbContext> dbContextFactory,
                  EventNotificationService eventNotificationService) : WidgetSparklineBase<Size.Data, object>
{
    public record Data(DateTime Date, double Size);

    protected override async Task OnInitializedAsync()
    {
        SerieTitle = L["Size"];
        CategoryProperty = nameof(Data.Date);
        ValueProperty = nameof(Data.Size);
        ValueFormatter = value => FormatHelper.FromBytes((double)value);

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
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var results = db.JobResults.Where(a => ClusterNames.Contains(a.TaskResult.ClusterName), ClusterNames.Any())
                                   .Where(a => a.TaskResult.Start > DateTime.UtcNow.AddDays(-60 * 3) && a.TaskResult.End != null
                                              && a.Size > 0);

        Items = [.. results.GroupBy(a => a.Start!.Value.Date)
                           .Select(a => new Data(a.Key, a.Sum(b => b.Size)))];

        LastExecution = results.OrderByDescending(a => a.Start).FirstOrDefault()?.Start;
        MakeFooterText($"{L["Last"]}: {(LastExecution.HasValue ? LastExecution.Value.ToLocalTime().ToString("g") : string.Empty)}");
    }

    public override void Dispose()
    {
        eventNotificationService.Unsubscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        base.Dispose();
    }
}
