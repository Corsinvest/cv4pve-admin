namespace Corsinvest.ProxmoxVE.Admin.Core.Models.Parameters;

public record ParameterOptions(Func<DataSourceContext, Task<DataSourceResult>>? DataSource = null,
                               decimal? Min = null,
                               decimal? Max = null,
                               string? Placeholder = null)
{
    /// <summary>
    /// Create options with static string values for Select/MultiSelect
    /// </summary>
    public static ParameterOptions FromValues(params string[] values)
        => FromValues(values.AsEnumerable());

    /// <summary>
    /// Create options with static string values for Select/MultiSelect
    /// </summary>
    public static ParameterOptions FromValues(IEnumerable<string> values)
    {
        var data = values.Select(v => new { Value = v, Label = v }).ToList();
        return new ParameterOptions(
            DataSource: _ => Task.FromResult(new DataSourceResult(data, "Label", "Value"))
        );
    }

    /// <summary>
    /// Create options with key-value pairs for Select/MultiSelect
    /// </summary>
    public static ParameterOptions FromValues(IEnumerable<(string Value, string Label)> values)
    {
        var data = values.Select(v => new { v.Value, v.Label }).ToList();
        return new ParameterOptions(
            DataSource: _ => Task.FromResult(new DataSourceResult(data, "Label", "Value"))
        );
    }

    /// <summary>
    /// Create options from enum type for Select/MultiSelect
    /// </summary>
    public static ParameterOptions FromEnum<TEnum>() where TEnum : struct, Enum
    {
        var data = Enum.GetValues<TEnum>()
            .Select(e => new { Value = e.ToString(), Label = FormatEnumLabel(e.ToString()) })
            .ToList();
        return new ParameterOptions(
            DataSource: _ => Task.FromResult(new DataSourceResult(data, "Label", "Value"))
        );
    }

    /// <summary>
    /// Create options from enum type with custom labels
    /// </summary>
    public static ParameterOptions FromEnum<TEnum>(Func<TEnum, string> labelSelector) where TEnum : struct, Enum
    {
        var data = Enum.GetValues<TEnum>()
            .Select(e => new { Value = e.ToString(), Label = labelSelector(e) })
            .ToList();
        return new ParameterOptions(
            DataSource: _ => Task.FromResult(new DataSourceResult(data, "Label", "Value"))
        );
    }

    private static string FormatEnumLabel(string enumValue) =>
        // Convert PascalCase to "Pascal Case"
        System.Text.RegularExpressions.Regex.Replace(enumValue, "([a-z])([A-Z])", "$1 $2");
}
