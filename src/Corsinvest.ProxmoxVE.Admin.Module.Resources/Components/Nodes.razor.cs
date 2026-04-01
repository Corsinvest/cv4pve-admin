/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components;

public partial class Nodes : IClusterName
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;
    [SupplyParameterFromQuery] public string? Node { get; set; }

    private ResourcesView? ResourcesExRef { get; set; }
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
                                             [nameof(ClusterResourceItem.Status),
                                              nameof(ClusterResourceItem.Description),
                                              nameof(ClusterResourceItem.CpuUsagePercentage),
                                              nameof(ClusterResourceItem.MemoryUsagePercentage),
                                              nameof(ClusterResourceItem.DiskUsagePercentage),
                                              nameof(ClusterResourceItem.HealthScore),
                                              nameof(ClusterResourceItem.Uptime)]);
}
