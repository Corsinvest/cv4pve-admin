namespace Corsinvest.ProxmoxVE.Admin.Core.Models.Parameters;

public record ParameterMetadata(string Id,
                                string Label,
                                string? Description,
                                ParameterType Type,
                                bool Required,
                                object? DefaultValue,
                                ParameterOptions? Options)
{
    public string ToString(Dictionary<string, object?> parameters) => parameters[Id] + string.Empty;
    public bool ToBoolean(Dictionary<string, object?> parameters) => Convert.ToBoolean(parameters[Id] ?? false);
    public long ToLong(Dictionary<string, object?> parameters) => Convert.ToInt64(parameters[Id] ?? 0);
}
