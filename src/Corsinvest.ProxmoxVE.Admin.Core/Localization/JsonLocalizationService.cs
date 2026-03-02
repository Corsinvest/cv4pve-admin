/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace Corsinvest.ProxmoxVE.Admin.Core.Localization;

public class JsonLocalizationService(IFusionCache cache) : IStringLocalizer
{
    private static readonly ConcurrentDictionary<string, byte> _discoveredKeys = new();
    private static readonly SemaphoreSlim _fileLock = new(1, 1);

    public LocalizedString this[string name] => GetTranslation(name);
    public LocalizedString this[string name, params object[] arguments] => GetTranslation(name, arguments);

    private LocalizedString GetTranslation(string key, params object[] args)
    {
        var culture = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

        var translations = cache.GetOrSet($"loc:{culture}", _ => LoadAllTranslations(culture), TimeSpan.FromHours(24));

        string? translatedValue = null;
        translations?.TryGetValue(key, out translatedValue);

        if (string.IsNullOrEmpty(translatedValue))
        {
#if DEBUG
            _ = RegisterMissingSourceKey(key);
#endif
            translatedValue = key;
        }

        var finalValue = translatedValue;
        if (args is { Length: > 0 })
        {
            try { finalValue = string.Format(translatedValue, args); }
            catch { }
        }

        return new LocalizedString(key, finalValue, translatedValue == key);
    }

    private static Dictionary<string, string> LoadAllTranslations(string culture)
    {
        var result = new Dictionary<string, string>();
        LoadFromEmbeddedResource(culture, result);
        LoadFromFile(Path.Combine(ApplicationHelper.TranslationsPath, $"{culture}.json"), result);
        return result;
    }

    private static void LoadFromEmbeddedResource(string culture, Dictionary<string, string> dict)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.translations.{culture}.json");
        if (stream == null) { return; }

        using StreamReader reader = new(stream);
        foreach (var kv in JsonSerializer.Deserialize<Dictionary<string, string>>(reader.ReadToEnd()) ?? [])
        {
            dict[kv.Key] = kv.Value;
        }
    }

    private static void LoadFromFile(string path, Dictionary<string, string> dict)
    {
        if (!File.Exists(path)) { return; }

        try
        {
            var json = File.ReadAllText(path);
            foreach (var kv in JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [])
            {
                dict[kv.Key] = kv.Value;
            }
        }
        catch { }
    }

    private static string FindSourceJsonPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "ce", "src", "Corsinvest.ProxmoxVE.Admin.Core", "translations", "source.json");
            if (File.Exists(candidate)) { return candidate; }
            dir = dir.Parent;
        }
        return Path.Combine(ApplicationHelper.TranslationsPath, "source.json");
    }

    private static async Task RegisterMissingSourceKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || _discoveredKeys.ContainsKey(key)) { return; }

        await _fileLock.WaitAsync();
        try
        {
            if (_discoveredKeys.ContainsKey(key)) { return; }

            _discoveredKeys.TryAdd(key, 0);

            var path = FindSourceJsonPath();
            var sourceData = File.Exists(path)
                                ? JsonSerializer.Deserialize<Dictionary<string, string>>(await File.ReadAllTextAsync(path)) ?? []
                                : [];

            if (!sourceData.ContainsKey(key))
            {
                sourceData[key] = "";
                await File.WriteAllTextAsync(path, JsonSerializer.Serialize(sourceData, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        finally { _fileLock.Release(); }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
}
