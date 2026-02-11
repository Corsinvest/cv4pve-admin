using Corsinvest.ProxmoxVE.Admin.Core.Notifier;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Settings;

public partial class NotifierConfigurationSelector<T>(INotifierService notifierService) where T : INotifierConfigurationsSettings, new()
{
    [Parameter] public T Settings { get; set; } = default!;

    private IEnumerable<Item> Items { get; set; } = [];
    private record Item(string Name, string Type, string Icon);

    protected override void OnInitialized()
        => Items = [.. notifierService.Modules.Select(a => notifierService.GetConfigurations(a)
                                                                          .IsEnabled()
                                                                          .Select(b => new Item(b.Name, a.Name, a.Icon!)))
                                                                          .SelectMany(a=> a)];
}
