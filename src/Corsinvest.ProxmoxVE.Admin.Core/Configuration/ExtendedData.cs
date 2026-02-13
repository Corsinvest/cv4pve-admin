/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Globalization;
using System.Text.Json;

namespace Corsinvest.ProxmoxVE.Admin.Core.Configuration;

public class ExtendedData : Dictionary<string, string>
{
    public string Get(string key, string defaultValue = "") => TryGetValue(key, out var value) ? value : defaultValue;

    public T Get<T>(string key, T defaultValue = default!)
    {
        if (!TryGetValue(key, out var value)) { return defaultValue; }

        try
        {
            var t = typeof(T);

            if (t == typeof(string)) { return (T)(object)value; }
            if (t == typeof(int)) { return (T)(object)int.Parse(value, CultureInfo.InvariantCulture); }
            if (t == typeof(bool)) { return (T)(object)bool.Parse(value); }
            if (t == typeof(double)) { return (T)(object)double.Parse(value, CultureInfo.InvariantCulture); }
            return t == typeof(DateTime)
                ? (T)(object)DateTime.Parse(value, CultureInfo.InvariantCulture)
                : JsonSerializer.Deserialize<T>(value) ?? defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    public void Set(string key, string value) => this[key] = value;

    public void Set<T>(string key, T? value)
    {
        if (value is null)
        {
            Remove(key);
            return;
        }

        this[key] = value switch
        {
            string s => s,
            int or bool or double or DateTime =>
                Convert.ToString(value, CultureInfo.InvariantCulture)!,
            _ => JsonSerializer.Serialize(value)
        };
    }
}
