/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using CsvHelper;
using Cysharp.AI;

namespace Corsinvest.ProxmoxVE.Admin.Module.AIServer.Helpers;

/// <summary>
/// Serializes flat lists for AI tool responses.
/// </summary>
public static class ToolHelper
{
    private static readonly JsonSerializerOptions _opts = new() { PropertyNamingPolicy = null };

    /// <summary>
    /// Serializes a list of objects wrapped in a key.
    /// JsonCompact (default): {"key":{"headers":[...],"rows":[[...],...]}}
    /// JsonNormal:            {"key":[{"field":value,...},...]}
    /// Toon:                  TOON tabular format via ToonEncoder
    /// Csv:                   header row + data rows, comma-separated
    /// </summary>
    private const string WrapperKey = "data";

    public static string SerializeTable<T>(IEnumerable<T> items, ToolOutputFormat format)
    {
        var list = items.ToList();
        var wrapper = new Dictionary<string, object> { [WrapperKey] = list };

        switch (format)
        {
            case ToolOutputFormat.JsonNormal: return JsonSerializer.Serialize(wrapper, _opts);
            case ToolOutputFormat.Toon: return ToonEncoder.Encode(wrapper);

            case ToolOutputFormat.Csv:
                using (var sw = new StringWriter())
                using (var csv = new CsvWriter(sw, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(list);
                    return sw.ToString().TrimEnd();
                }

            default: return SerializeJsonCompact();
        }

        string SerializeJsonCompact()
        {
            var nodes = list.Select(item => JsonNode.Parse(JsonSerializer.Serialize(item, _opts))?.AsObject())
                            .OfType<JsonObject>()
                            .ToList();

            if (nodes.Count == 0)
            {
                return JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    [WrapperKey] = new { headers = Array.Empty<string>(), rows = Array.Empty<object>() }
                });
            }
            else
            {
                var headers = nodes[0].Select(a => a.Key).ToArray();

                return new JsonObject
                {
                    [WrapperKey] = new JsonObject
                    {
                        ["headers"] = new JsonArray([.. headers.Select(a => JsonValue.Create(a))]),
                        ["rows"] = new JsonArray([.. nodes.Select(n => (JsonNode)new JsonArray([.. headers.Select(a => n[a]?.DeepClone())]))])
                    }
                }.ToJsonString();
            }
        }
    }
}
