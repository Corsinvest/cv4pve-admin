/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Cluster;
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Microsoft.AspNetCore.WebUtilities;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components;

public partial class Nodes : IClusterName
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private ResourcesEx? ResourcesExRef { get; set; }
    private DataGridSettings DataGridSettings { get; set; } = new();

    private bool _expanded;
    private async Task OnDataLoadedAsync()
    {
        if (_expanded) { return; }
        var query = QueryHelpers.ParseQuery(new Uri(NavigationManager.Uri).Query);
        if (query.TryGetValue("node", out var node) && !string.IsNullOrEmpty(node))
        {
            _expanded = true;
            await ResourcesExRef!.ExpandRowsAsync(ResourcesExRef.GetItems().Where(a => a.Node == node));
        }
    }

    protected override void OnInitialized()
        => RadzenHelper.MakeDataGridSettings(DataGridSettings,
                                             [nameof(ClusterResourceEx.Status),
                                              nameof(ClusterResourceEx.Description),
                                              nameof(ClusterResourceEx.CpuUsagePercentage),
                                              nameof(ClusterResourceEx.MemoryUsagePercentage),
                                              nameof(ClusterResourceEx.DiskUsagePercentage),
                                              nameof(ClusterResourceEx.HealthScore),
                                              nameof(ClusterResourceEx.Uptime)]);
}
