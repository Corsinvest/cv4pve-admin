namespace Corsinvest.ProxmoxVE.Admin.Core.Notifier;

internal class NotifierService(IModuleService moduleService,
                               ISettingsService settingsService,
                               IServiceProvider serviceProvider,
                               ILogger<NotifierService> Logger) : INotifierService
{
    public IEnumerable<ModuleBase> Modules => moduleService.Modules.Where(a => a.ModuleType == ModuleType.Notification);

    public IEnumerable<NotifierConfiguration> GetConfigurations()
        => [.. Modules.Select(a => GetConfigurations(a).IsEnabled()).SelectMany(a => a)];

    public IEnumerable<NotifierConfiguration> Get(Type type)
        => (IEnumerable<NotifierConfiguration>)settingsService.Get(typeof(List<>).MakeGenericType(type),
                                                                         typeof(NotifierConfiguration).FullName!,
                                                                         type.FullName!,
                                                                         false);

    public async Task SetAsync(Type setttinsType, IEnumerable<NotifierConfiguration> notifiers)
        => await settingsService.SetAsync(typeof(NotifierConfiguration).FullName!,
                                          setttinsType.FullName!,
                                          notifiers,
                                          false);

    private static Type GetType(ModuleBase module) => (Type)module.Attributes["TypeDef"]!;

    public IEnumerable<NotifierConfiguration> GetConfigurations(ModuleBase module) => Get(GetType(module));

    public async Task SendAsync(IEnumerable<string> notifiers, NotifierMessage message)
    {
        foreach (var item in Modules.SelectMany(a => Get(GetType(a)))
                                    .Where(a => a.Enabled && notifiers.Contains(a.Name)))
        {
            try
            {
                await item.SendAsync(message, serviceProvider);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, nameof(SendAsync));
            }
        }
    }
}
