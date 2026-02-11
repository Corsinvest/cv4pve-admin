using Corsinvest.ProxmoxVE.Admin.Core.Models.Parameters;

namespace Corsinvest.ProxmoxVE.Admin.Core.Search.Providers;

public class SystemSearchProvider : ISearchProvider
{
    public string Id => "SystemSearch";
    public string DisplayName => "System";
    public bool RequireClusterName => false;

    public IEnumerable<SearchFilter> Filters => [];
    public IEnumerable<SearchCommand> Commands =>
    [
        new("logout", "Logout", "Logout from application", "logout", [], LogoutAsync),
    ];

    private Task LogoutAsync(CommandExecutionContext context)
    {
        var navigationManager = context.ServiceProvider.GetRequiredService<NavigationManager>();
        navigationManager.NavigateTo("/logout", forceLoad: true);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<SearchResultItem>> SearchAsync(SearchContext context)
        => context.IsCommandSearch
            ? SearchResultItem.CommandsToResult(this)
                              .Where(a => a.MatchesSearch(context.SearchText))
            : [];

    public Task<DataSourceResult> GetDataSourceDialogAsync(string dataSource, DataSourceContext context)
        => Task.FromResult(DataSourceResult.Empty);
}
