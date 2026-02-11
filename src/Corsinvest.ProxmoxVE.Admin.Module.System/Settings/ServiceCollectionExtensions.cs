using Corsinvest.ProxmoxVE.Admin.Module.System.Settings.Services;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Settings;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSettingsAdmin(this IServiceCollection services)
        => services.AddScoped<ISettingsService, SettingsService>();
}
