/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Cluster;
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components;

public partial class Nodes : IClusterName
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;
    [SupplyParameterFromQuery] public string? Node { get; set; }

    private ResourcesEx? ResourcesExRef { get; set; }
    private DataGridSettings DataGridSettings { get; set; } = new();

    private bool _expanded;
    private async Task OnDataLoadedAsync()
    {
        if (_expanded) { return; }
        if (!string.IsNullOrEmpty(Node))
        {
            _expanded = true;
            await ResourcesExRef!.ExpandRowsAsync(ResourcesExRef.GetItems().Where(a => a.Node == Node));
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
