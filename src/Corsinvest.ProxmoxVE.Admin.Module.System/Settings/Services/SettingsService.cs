using System.Text.Json;
using ZiggyCreatures.Caching.Fusion;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Settings.Services;

public class SettingsService(IDbContextFactory<ModuleDbContext> dbContextFactory,
                             IHttpContextAccessor httpContextAccessor,
                             IFusionCache fusionCache,
                             ILogger<SettingsService> logger,
                             JsonSerializerOptions serializerOptions) : ISettingsService
{
    private string UserName => httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "anonymous";

    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(30);
    private const string CacheKeyPrefix = "settings";
    private const string CacheTag = "Settings";

    private string GetContext(bool forUser)
        => forUser
            ? $"User:{UserName}"
            : "System";

    private static string GetCacheKey(string context, string section, string key)
        => $"{CacheKeyPrefix}:{context}:{section}:{key}";

    public object Get(Type settingsType, string section, string key, bool forCurrentUser)
    {
        ArgumentNullException.ThrowIfNull(settingsType);
        ArgumentException.ThrowIfNullOrWhiteSpace(section);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var context = GetContext(forCurrentUser);
        var cacheKey = GetCacheKey(context, section, key);

        return fusionCache.GetOrSet(cacheKey, _ =>
        {
            try
            {
                using var db = dbContextFactory.CreateDbContext();
                var value = db.Settings
                              .AsNoTracking()
                              .Where(a => a.Context == context && a.Section == section && a.Key == key)
                              .Select(a => a.Value)
                              .FirstOrDefault();

                return DeserializeOrDefault(value, settingsType, section, key, context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                                "Database error while retrieving settings for section '{Section}', key '{Key}', context '{Context}'",
                                section,
                                key,
                                context);
                return CreateDefaultInstance(settingsType);
            }
        }, CacheExpiration, tags: [CacheTag]);
    }

    public TSetting Get<TSetting>(bool forCurrentUser = false)
        => Get<TSetting>(typeof(TSetting).FullName!, typeof(TSetting).FullName!, forCurrentUser);

    public TSetting Get<TSetting>(string section, string key, bool forCurrentUser)
        => (TSetting)Get(typeof(TSetting), section, key, forCurrentUser);

    public async Task SetAsync(string section, string key, object value, bool forCurrentUser)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(section);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        var context = GetContext(forCurrentUser);
        var cacheKey = GetCacheKey(context, section, key);

        try
        {
            var serializedValue = JsonSerializer.Serialize(value, serializerOptions);

            await using var db = await dbContextFactory.CreateDbContextAsync();
            var item = await db.Settings
                               .FirstOrDefaultAsync(a => a.Context == context && a.Section == section && a.Key == key);

            if (item == null)
            {
                item = new()
                {
                    Section = section,
                    Key = key,
                    Context = context
                };
                await db.Settings.AddAsync(item);
            }

            item.Value = serializedValue;

            await db.SaveChangesAsync();

            await fusionCache.SetAsync(cacheKey, value, CacheExpiration, tags: [CacheTag]);

            logger.LogDebug("Settings saved successfully for section '{Section}', key '{Key}', context '{Context}'",
                            section,
                            key,
                            context);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex,
                            "Failed to serialize settings value for section '{Section}', key '{Key}', context '{Context}'",
                            section,
                            key,
                            context);
            throw new InvalidOperationException($"Cannot serialize settings value for {section}:{key}", ex);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex,
                            "Database error while saving settings for section '{Section}', key '{Key}', context '{Context}'",
                            section,
                            key,
                            context);
            throw;
        }
    }

    public async Task SetAsync<TSetting>(TSetting value, bool forCurrentUser = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        await SetAsync(typeof(TSetting).FullName!, typeof(TSetting).FullName!, value, forCurrentUser);
    }

    //public void Clear(string section, string key, bool forCurrentUser = false)
    //{
    //    ArgumentException.ThrowIfNullOrWhiteSpace(section);
    //    ArgumentException.ThrowIfNullOrWhiteSpace(key);

    //    var context = GetContext(forCurrentUser);
    //    var cacheKey = GetCacheKey(context, section, key);

    //    try
    //    {
    //        var item = db.UserSettings
    //                    .FirstOrDefault(a => a.Context == context && a.Section == section && a.Key == key);

    //        if (item != null)
    //        {
    //            db.UserSettings.Remove(item);
    //            db.SaveChanges();

    //            logger.LogDebug("Settings removed from database for section '{Section}', key '{Key}', context '{Context}'",
    //                            section,
    //                            key,
    //                            context);
    //        }

    //        // Rimuovi dalla cache indipendentemente dal database
    //        fusionCache.Remove(cacheKey);
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex,
    //                        "Error while clearing settings for section '{Section}', key '{Key}', context '{Context}'",
    //                        section,
    //                        key,
    //                        context);
    //        throw;
    //    }
    //}

    //public async Task<TSettings> GetAsync<TSettings>(bool forCurrentUser = false) where TSettings : new()
    //    => await GetAsync<TSettings>(typeof(TSettings).FullName!, typeof(TSettings).FullName!, forCurrentUser);

    //public async Task<TSettings> GetAsync<TSettings>(string section, string key, bool forCurrentUser) where TSettings : new()
    //{
    //    ArgumentException.ThrowIfNullOrWhiteSpace(section);
    //    ArgumentException.ThrowIfNullOrWhiteSpace(key);

    //    var context = GetContext(forCurrentUser);
    //    var cacheKey = GetCacheKey(context, section, key);

    //    return await fusionCache.GetOrSetAsync(cacheKey, async _ =>
    //    {
    //        try
    //        {
    //            var value = await db.UserSettings
    //                               .AsNoTracking()
    //                               .Where(a => a.Context == context && a.Section == section && a.Key == key)
    //                               .Select(a => a.Value)
    //                               .FirstOrDefaultAsync();

    //            return DeserializeOrDefault<TSettings>(value, section, key, context);
    //        }
    //        catch (Exception ex)
    //        {
    //            logger.LogError(ex,
    //                            "Database error while retrieving settings of type {Type} for section '{Section}', key '{Key}', context '{Context}'",
    //                            typeof(TSettings).Name,
    //                            section,
    //                            key,
    //                            context);
    //            return new TSettings();
    //        }
    //    }, CacheExpiration);
    //}

    //public async Task ClearAsync(string section, string key, bool forCurrentUser = false)
    //{
    //    ArgumentException.ThrowIfNullOrWhiteSpace(section);
    //    ArgumentException.ThrowIfNullOrWhiteSpace(key);

    //    var context = GetContext(forCurrentUser);
    //    var cacheKey = GetCacheKey(context, section, key);

    //    try
    //    {
    //        var item = await db.UserSettings
    //                          .FirstOrDefaultAsync(a => a.Context == context && a.Section == section && a.Key == key);

    //        if (item != null)
    //        {
    //            db.UserSettings.Remove(item);
    //            await db.SaveChangesAsync();

    //            logger.LogDebug("Settings removed from database for section '{Section}', key '{Key}', context '{Context}'",
    //                            section,
    //                            key,
    //                            context);
    //        }

    //        // Rimuovi dalla cache indipendentemente dal database
    //        await fusionCache.RemoveAsync(cacheKey);
    //    }
    //    catch (DbUpdateException ex)
    //    {
    //        logger.LogError(ex,
    //                        "Database error while removing settings for section '{Section}', key '{Key}', context '{Context}'",
    //                        section,
    //                        key,
    //                        context);
    //        throw;
    //    }
    //}

    //public async Task ClearSectionAsync(string section, bool forCurrentUser = false)
    //{
    //    ArgumentException.ThrowIfNullOrWhiteSpace(section);

    //    var context = GetContext(forCurrentUser);

    //    try
    //    {
    //        var items = await db.UserSettings
    //                           .Where(a => a.Context == context && a.Section == section)
    //                           .ToListAsync();

    //        if (items.Count != 0)
    //        {
    //            db.UserSettings.RemoveRange(items);
    //            await db.SaveChangesAsync();

    //            // Rimuovi tutte le chiavi della sezione dalla cache
    //            foreach (var item in items)
    //            {
    //                var cacheKey = GetCacheKey(context, section, item.Key);
    //                await fusionCache.RemoveAsync(cacheKey);
    //            }

    //            logger.LogDebug("Cleared {Count} settings from section '{Section}', context '{Context}'",
    //                            items.Count,
    //                            section,
    //                            context);
    //        }
    //    }
    //    catch (DbUpdateException ex)
    //    {
    //        logger.LogError(ex,
    //                        "Database error while clearing section '{Section}', context '{Context}'",
    //                        section,
    //                        context);
    //        throw;
    //    }
    //}

    //public async Task ClearUserSettingsAsync(string? userName = null)
    //{
    //    var targetUser = userName ?? UserName;
    //    var context = $"User:{targetUser}";

    //    try
    //    {
    //        var items = await db.UserSettings
    //                           .Where(a => a.Context == context)
    //                           .ToListAsync();

    //        if (items.Any())
    //        {
    //            db.UserSettings.RemoveRange(items);
    //            await db.SaveChangesAsync();

    //            // Pulisci cache manualmente per ogni item
    //            foreach (var item in items)
    //            {
    //                var cacheKey = GetCacheKey(context, item.Section, item.Key);
    //                await fusionCache.RemoveAsync(cacheKey);
    //            }

    //            logger.LogInformation("Cleared {Count} settings for user '{UserName}'", items.Count, targetUser);
    //        }
    //    }
    //    catch (DbUpdateException ex)
    //    {
    //        logger.LogError(ex, "Database error while clearing settings for user '{UserName}'", targetUser);
    //        throw;
    //    }
    //}

    //public async Task<Dictionary<string, TSettings>> GetSectionAsync<TSettings>(string section, bool forCurrentUser = false) where TSettings : new()
    //{
    //    ArgumentException.ThrowIfNullOrWhiteSpace(section);

    //    var context = GetContext(forCurrentUser);

    //    try
    //    {
    //        var items = await db.UserSettings
    //                           .AsNoTracking()
    //                           .Where(a => a.Context == context && a.Section == section)
    //                           .Select(a => new { a.Key, a.Value })
    //                           .ToListAsync();

    //        var result = new Dictionary<string, TSettings>();

    //        foreach (var item in items)
    //        {
    //            result[item.Key] = DeserializeOrDefault<TSettings>(item.Value, section, item.Key, context);
    //        }

    //        return result;
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "Error retrieving section '{Section}' for context '{Context}'", section, context);
    //        return [];
    //    }
    //}

    private object DeserializeOrDefault(string? value, Type settingsType, string section, string key, string context)
    {
        if (string.IsNullOrEmpty(value)) { return CreateDefaultInstance(settingsType); }

        try
        {
            return JsonSerializer.Deserialize(value, settingsType, serializerOptions) ?? CreateDefaultInstance(settingsType);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex,
                              "Failed to deserialize settings for section '{Section}', key '{Key}', context '{Context}'. Using default value. JSON: {Json}",
                              section,
                              key,
                              context,
                              value);

            //// Rimozione automatica del valore corrotto in background
            //_ = Task.Run(async () =>
            //{
            //    try
            //    {
            //        await RemoveCorruptedSettingAsync(context, section, key);
            //    }
            //    catch (Exception removeEx)
            //    {
            //        logger.LogError(removeEx, "Failed to remove corrupted setting");
            //    }
            //});

            return CreateDefaultInstance(settingsType);
        }
    }

    //private TSettings DeserializeOrDefault<TSettings>(string? value, string section, string key, string context) where TSettings : new()
    //{
    //    if (string.IsNullOrEmpty(value)) { return new TSettings(); }

    //    try
    //    {
    //        return JsonSerializer.Deserialize<TSettings>(value, SerializerOptions) ?? new TSettings();
    //    }
    //    catch (JsonException ex)
    //    {
    //        logger.LogWarning(ex,
    //                          "Failed to deserialize settings of type {Type} for section '{Section}', key '{Key}', context '{Context}'. Using default value. JSON: {Json}",
    //                          typeof(TSettings).Name,
    //                          section,
    //                          key,
    //                          context,
    //                          value);

    //        // Rimozione automatica del valore corrotto in background
    //        _ = Task.Run(async () =>
    //        {
    //            try
    //            {
    //                await RemoveCorruptedSettingAsync(context, section, key);
    //            }
    //            catch (Exception removeEx)
    //            {
    //                logger.LogError(removeEx, "Failed to remove corrupted setting");
    //            }
    //        });

    //        return new TSettings();
    //    }
    //}
    private static object CreateDefaultInstance(Type settingType)
    {
        try
        {
            return Activator.CreateInstance(settingType) ??
                   throw new InvalidOperationException($"Cannot create instance of {settingType.Name}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create default instance of type {settingType.FullName}", ex);
        }
    }

    //private async Task RemoveCorruptedSettingAsync(string context, string section, string key)
    //{
    //    try
    //    {
    //        var corruptedItem = await db.UserSettings
    //                                    .FirstOrDefaultAsync(a => a.Context == context && a.Section == section && a.Key == key);

    //        if (corruptedItem != null)
    //        {
    //            db.UserSettings.Remove(corruptedItem);
    //            await db.SaveChangesAsync();

    //            var cacheKey = GetCacheKey(context, section, key);
    //            await fusionCache.RemoveAsync(cacheKey);

    //            logger.LogInformation("Removed corrupted setting: {Context}:{Section}:{Key}", context, section, key);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "Failed to remove corrupted setting {Context}:{Section}:{Key}", context, section, key);
    //    }
    //}

    public ValueTask ClearCacheAsync() => fusionCache.RemoveByTagAsync(CacheTag);
}
