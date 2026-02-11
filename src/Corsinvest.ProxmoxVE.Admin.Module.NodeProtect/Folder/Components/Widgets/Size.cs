using Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;
using Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Persistence;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Folder.Components.Widgets;

public class Size(ISettingsService settingsService,
                  IDbContextFactory<ModuleDbContext> dbContextFactory,
                  EventNotificationService eventNotificationService) : WidgetSparklineBase<Size.Data, object>
{
    private bool IsConfigured { get; set; }

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
        var clusterNames = ClusterNames.Any()
            ? ClusterNames
            : settingsService.GetEnabledClustersSettings().Select(a => a.Name);

        IsConfigured = clusterNames.Any(clusterName =>
        {
            var settings = settingsService.GetForModule<Module, Settings>(clusterName);
            return settings.Enabled && settings.Folder.Enabled;
        });

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var results = db.FolderTaskResults
                        .Where(a => clusterNames.Contains(a.ClusterName), clusterNames.Any())
                        .Where(a => a.Start > DateTime.UtcNow.AddDays(-60 * 3) && a.End != null && a.Size > 0);

        Items = [.. results.GroupBy(a => a.Start.Date)
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
