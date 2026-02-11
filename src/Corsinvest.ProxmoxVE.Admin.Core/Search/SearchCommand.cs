using Corsinvest.ProxmoxVE.Admin.Core.Models.Parameters;

namespace Corsinvest.ProxmoxVE.Admin.Core.Search;

public record SearchCommand(
    string Id,
    string Label,
    string Description,
    string Icon,
    ParameterMetadata[] Parameters,
    Func<CommandExecutionContext, Task> Execute)
{
    public string Prefix => $">{Id}";
    public bool RequiresDialog => Parameters.Length > 0;
}
