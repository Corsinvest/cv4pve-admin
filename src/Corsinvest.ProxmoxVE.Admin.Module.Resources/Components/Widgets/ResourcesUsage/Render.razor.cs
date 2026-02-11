using Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Cluster;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components.Widgets.ResourcesUsage;

public partial class Render : IModuleWidget<Settings>
{
    [Parameter] public Settings Settings { get; set; } = default!;
    [Parameter] public EventCallback<Settings> SettingsChanged { get; set; }
    [Parameter] public IEnumerable<string> ClusterNames { get; set; } = [];
    [Parameter] public bool InEditing { get; set; }

    private ResourcesEx? ResourcesExRef { get; set; }
    private bool ShowGrid { get; set; }

    private enum ResourcePreset { Guest, Node, Storage }

    private ResourcePreset? _selectedPreset;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender && !ShowGrid)
        {
            ShowGrid = true;
            await InvokeAsync(StateHasChanged);
        }
    }

    public async Task RefreshDataAsync()
    {
        if (ResourcesExRef != null) { await ResourcesExRef.RefreshDataAsync(); }
    }

    private async Task DataGridSettingsAfter() => await SettingsChanged.InvokeAsync(Settings);

    private async Task OnPresetChanged()
    {
        if (_selectedPreset == null) { return; }

        // Force columns sync if not yet populated
        if (!Settings.DataGridSettings.Columns.Any() && ResourcesExRef != null)
        {
            ResourcesExRef.SaveSettings();
        }
        var settings = Settings.DataGridSettings;

        // Configure based on preset
        string[] visibleColumns;
        string[] typeFilter;
        List<GroupDescriptor> groups = [new() { Property = nameof(ClusterResourceEx.ClusterName), Title = "Cluster Name" }];

        switch (_selectedPreset)
        {
            case ResourcePreset.Guest:
                typeFilter = ["qemu", "lxc"];
                visibleColumns = [nameof(ClusterResourceEx.Status),
                                  nameof(ClusterResourceEx.Description),
                                  nameof(ClusterResourceEx.CpuUsagePercentage),
                                  nameof(ClusterResourceEx.MemoryUsagePercentage),
                                  nameof(ClusterResourceEx.DiskUsagePercentage),
                                  ClusterResourceEx.CommandsColumnName];
                break;

            case ResourcePreset.Node:
                typeFilter = ["node"];
                visibleColumns = [nameof(ClusterResourceEx.Status),
                                  nameof(ClusterResourceEx.Description),
                                  nameof(ClusterResourceEx.CpuUsagePercentage),
                                  nameof(ClusterResourceEx.MemoryUsagePercentage),
                                  nameof(ClusterResourceEx.DiskUsagePercentage),
                                  ClusterResourceEx.CommandsColumnName];
                break;

            case ResourcePreset.Storage:
                typeFilter = ["storage"];
                visibleColumns = [nameof(ClusterResourceEx.Status),
                                  nameof(ClusterResourceEx.Storage),
                                  nameof(ClusterResourceEx.PluginType),
                                  nameof(ClusterResourceEx.DiskUsagePercentage)];
                groups.Add(new() { Property = nameof(ClusterResourceEx.Node), Title = "Node" });
                break;

            default:
                return;
        }

        // Apply groups
        settings.Groups = groups;

        settings.Columns.FirstOrDefault(a => a.Property == nameof(ClusterResourceEx.Type))!.FilterValue = typeFilter;

        foreach (var col in settings.Columns)
        {
            col.Visible = visibleColumns.Contains(col.Property);
        }

        await SettingsChanged.InvokeAsync(Settings);
        _selectedPreset = null;
        if (ResourcesExRef != null) { await ResourcesExRef.ReloadSettingsAsync(); }
    }
}
