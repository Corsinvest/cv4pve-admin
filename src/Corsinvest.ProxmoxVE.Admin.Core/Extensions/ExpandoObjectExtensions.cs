using System.Dynamic;
using System.Text.Json;
using Mapster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class ExpandoObjectExtensions
{
    public static object To(this ExpandoObject expandoObject, Type type)
        => JsonSerializer.Deserialize(JsonSerializer.Serialize(expandoObject), type)!;

    public static void From(this ExpandoObject expandoObject, object? obj)
    {
        if (obj != null)
        {
            var dic = (IDictionary<string, object>)expandoObject!;
            var adaptedDictionary = obj.Adapt<Dictionary<string, object>>();

            foreach (var kvp in adaptedDictionary)
            {
                dic[kvp.Key] = kvp.Value;
            }
        }
    }
}
