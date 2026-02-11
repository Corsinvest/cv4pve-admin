using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Microsoft.AspNetCore.Components;

namespace Corsinvest.ProxmoxVE.Admin.Module.AIServer.Components;

public partial class Status(ISettingsService settingsService,
                            IModuleService moduleService) : IDisposable, IClusterName
{
    [CascadingParameter(Name = nameof(IClusterName.ClusterName))] public string ClusterName { get; set; } = default!;

    protected bool McpEnabled { get; set; }

    protected override void OnInitialized()
    {
        moduleService.Get<Module>()!.SettingsUpdated += SettingsUpdated;
        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = settingsService.GetForModule<Module, Settings>(ClusterName);
        McpEnabled = settings.Enabled;
        StateHasChanged();
    }

    private void SettingsUpdated(object? sender, EventArgs e) => LoadSettings();
    public void Dispose() => moduleService.Get<Module>()!.SettingsUpdated -= SettingsUpdated;
}
