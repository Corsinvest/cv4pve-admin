namespace Corsinvest.ProxmoxVE.Admin.Core.Search;

public record CommandExecutionContext(SearchCommand Command,
                                      Dictionary<string, object?> Parameters,
                                      string ClusterName,
                                      IServiceProvider ServiceProvider);
