using System.Text.Json;

namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Models;

/// <summary>
/// Represents a single condition or nested conditions in WHERE clause
/// </summary>
public class Condition : WhereClause
{
    public string Field { get; set; } = default!;
    public string Operator { get; set; } = default!;

    private IList<object> _value = [];
    public IList<object> Value
    {
        get => _value;
        set => _value = value?.Select(ConvertValue).ToList() ?? [];
    }

    public new Condition Clone()
        => new()
        {
            Logic = Logic,
            Conditions = [.. Conditions.Select(c => c.Clone())],
            Field = Field,
            Operator = Operator,
            Value = [.. Value]
        };

    private static object ConvertValue(object value)
        => value is JsonElement element
            ? element.ValueKind switch
            {
                JsonValueKind.String => element.GetString()!,
                JsonValueKind.Number => ConvertNumber(element),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => element
            }
            : value;

    private static object ConvertNumber(JsonElement element)
    {
        if (element.TryGetInt32(out var intValue)) { return intValue; }
        if (element.TryGetInt64(out var longValue)) { return longValue; }
        return element.TryGetDecimal(out var decimalValue)
            ? decimalValue
            : element.TryGetDouble(out var doubleValue)
                ? (object)doubleValue
                : throw new InvalidOperationException("Unknown number type in JSON element");
    }
}
