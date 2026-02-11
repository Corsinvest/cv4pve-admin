namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class SettingsServiceExtensions
{
    private static string GetContextModule(Type moduleType) => moduleType.FullName!;
    private static string GetKeyModule(Type settingType, string clusterName) => $"{settingType.FullName}-{clusterName}";

    public static TSetting GetForModule<TModule, TSetting>(this ISettingsService settingsService, string clusterName) where TModule : ModuleBase
        => (TSetting)settingsService.GetForModule(IModuleService.GetCached<TModule>()!.GetType(),
                                                   typeof(TSetting), clusterName);

    public static object GetForModule(this ISettingsService settingsService, ModuleBase module, string clusterName)
        => settingsService.GetForModule(module.GetType(), module.RenderSettingsInfo!.Type, clusterName);

    private static object GetForModule(this ISettingsService settingsService,
                                       Type moduleType,
                                       Type settingsType,
                                       string clusterName)
    {
        var ret = settingsService.Get(settingsType,
                                      GetContextModule(moduleType),
                                      GetKeyModule(settingsType, clusterName),
                                      false);
        if (ret is IClusterName retWithClusterName && string.IsNullOrEmpty(retWithClusterName.ClusterName))
        {
            retWithClusterName.ClusterName = clusterName;
        }

        return ret;
    }

    public static async Task SetAsync(this ISettingsService settingsService,
                                      ModuleBase module,
                                      string clusterName,
                                      object value)
        => await settingsService.SetAsync(module.GetType().FullName!,
                                          $"{module.RenderSettingsInfo!.Type.FullName}-{clusterName}",
                                          value,
                                          false);

    public static ClustersSettings GetClustersSettings(this ISettingsService settingsService)
        => settingsService.Get<ClustersSettings>();

    public static IEnumerable<ClusterSettings> GetEnabledClustersSettings(this ISettingsService settingsService)
        => settingsService.GetClustersSettings().IsEnabled();

    public static ClusterSettings? GetClusterSettings(this ISettingsService settingsService, string clusterName)
        => settingsService.GetEnabledClustersSettings().FromName(clusterName);

    public static AppSettings GetAppSettings(this ISettingsService settingsService)
        => settingsService.Get<AppSettings>();
}
