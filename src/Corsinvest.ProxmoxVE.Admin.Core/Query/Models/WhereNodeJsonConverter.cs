/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Models;

/// <summary>
/// Reads a WHERE element as a condition or as a nested group, based on the properties it carries
/// </summary>
public class WhereNodeJsonConverter : JsonConverter<WhereNode>
{
    public override WhereNode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var element = document.RootElement;
        if (element.ValueKind != JsonValueKind.Object) { throw new JsonException("A where condition must be a JSON object"); }

        var hasField = HasProperty(element, nameof(Condition.Field), options);
        var hasConditions = HasProperty(element, nameof(WhereClause.Conditions), options);

        switch (hasField, hasConditions)
        {
            case (true, false): return element.Deserialize<Condition>(options)!;

            case (false, true):
                var group = element.Deserialize<WhereClause>(options)!;
                return group.Conditions == null || group.Conditions.Count == 0
                        ? throw new JsonException("A nested where group must have at least one condition")
                        : group;

            case (true, true): throw new JsonException("A where condition cannot have both 'field' and 'conditions'");
            default: throw new JsonException("A where condition must have 'field' or 'conditions'");
        }
    }

    public override void Write(Utf8JsonWriter writer, WhereNode value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, value.GetType(), options);

    private static bool HasProperty(JsonElement element, string name, JsonSerializerOptions options)
    {
        var comparison = options.PropertyNameCaseInsensitive
                            ? StringComparison.OrdinalIgnoreCase
                            : StringComparison.Ordinal;

        return element.EnumerateObject().Any(a => a.Name.Equals(name, comparison));
    }
}
