/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Layout;

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
    private string? DisplayName { get; set; }
    private string? FullDisplayName { get; set; }
    private string SearchString { get; set; } = default!;
    private RadzenTextBox SearchBoxRef { get; set; } = default!;

    private IEnumerable<ClusterSettings> Data
        => settingsService.GetEnabledClustersSettings()
                          .Where(a => a.FullDisplayName.Contains(SearchString), !string.IsNullOrWhiteSpace(SearchString));

    private void RefreshData()
    {
        var clusterSettings = settingsService.GetClusterSettings(ClusterName);
        DisplayName = clusterSettings?.DisplayName ?? L["Select Cluster..."];
        FullDisplayName = clusterSettings?.FullDisplayName;
        Icon = clusterSettings?.TypeIcon;
    }

    private async Task OnMenuClick(RadzenProfileMenuItem item)
    {
        if (item.Value is string clusterName)
        {
            ClusterName = clusterName;
            if (ClusterNameChanged.HasDelegate) { await ClusterNameChanged.InvokeAsync(clusterName); }
        }
    }
}
