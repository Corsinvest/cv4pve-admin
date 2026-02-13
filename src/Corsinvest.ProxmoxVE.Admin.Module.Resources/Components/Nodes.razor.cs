/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components;

public partial class Nodes : IClusterName
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private DataGridSettings DataGridSettings { get; set; } = new();

    protected override void OnInitialized()
        => RadzenHelper.MakeDataGridSettings(DataGridSettings,
                                             [nameof(ClusterResourceEx.Status),
                                              nameof(ClusterResourceEx.Description),
                                              nameof(ClusterResourceEx.CpuUsagePercentage),
                                              nameof(ClusterResourceEx.MemoryUsagePercentage),
                                              nameof(ClusterResourceEx.DiskUsagePercentage),
                                              nameof(ClusterResourceEx.Uptime)]);
}
