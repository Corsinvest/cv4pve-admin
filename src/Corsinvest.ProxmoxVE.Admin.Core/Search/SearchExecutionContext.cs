namespace Corsinvest.ProxmoxVE.Admin.Core.Search;

public record SearchExecutionContext(SearchResultItem Item,
                                     string ClusterName,
                                     NavigationManager NavigationManager);
