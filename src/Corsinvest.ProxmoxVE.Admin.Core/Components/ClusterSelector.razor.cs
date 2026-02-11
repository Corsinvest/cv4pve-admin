using Radzen.Blazor.Rendering;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

public partial class ClusterSelector(ISettingsService settingsService) : IClusterName
{
    private string _clusterName = default!;
    [Parameter]
    public string ClusterName
    {
        get => _clusterName;
        set
        {
            _clusterName = value;
            RefreshData();
        }
    }

    [Parameter] public EventCallback<string> ClusterNameChanged { get; set; } = default!;

    private string? Icon { get; set; }
    private string? Type { get; set; }
    private string? Description { get; set; }
    private string? FullNamePart1 { get; set; }
    private string? FullName { get; set; }
    private string SearchString { get; set; } = default!;
    private Popup PopupRef { get; set; } = default!;
    private RadzenButton ButtonRef { get; set; } = default!;

    private IEnumerable<ClusterSettings> Data
        => settingsService.GetEnabledClustersSettings()
                          .Where(a => a.FullName.Contains(SearchString), !string.IsNullOrWhiteSpace(SearchString));

    private void RefreshData()
    {
        var clusterSetttings = settingsService.GetClusterSettings(ClusterName);
        FullNamePart1 = clusterSetttings?.FullNamePart1 ?? L["Select Cluster..."];
        Description = clusterSetttings?.Description;
        FullName = clusterSetttings?.FullName;
        Icon = clusterSetttings?.Icon;
        Type = clusterSetttings?.DecodeType;
    }

    private async Task SelectAsync(string clusterName)
    {
        await PopupRef.CloseAsync();
        ClusterName = clusterName;

        if (ClusterNameChanged.HasDelegate) { await ClusterNameChanged.InvokeAsync(clusterName); }
    }
}
