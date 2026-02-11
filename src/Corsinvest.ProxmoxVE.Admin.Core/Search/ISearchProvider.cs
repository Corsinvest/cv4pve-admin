namespace Corsinvest.ProxmoxVE.Admin.Core.Search;

public interface ISearchProvider
{
    string Id { get; }
    string DisplayName { get; }
    bool RequireClusterName { get; }
    IEnumerable<SearchFilter> Filters { get; }
    IEnumerable<SearchCommand> Commands { get; }
    Task<IEnumerable<SearchResultItem>> SearchAsync(SearchContext context);
}
