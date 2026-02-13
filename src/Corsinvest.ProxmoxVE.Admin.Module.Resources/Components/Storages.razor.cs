/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components;

public partial class Storages : IClusterName
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private DataGridSettings DataGridSettingsStorage { get; set; } = new();
    private DataGridSettings DataGridSettingsVm { get; set; } = new();

    protected override void OnInitialized()
    {
        RadzenHelper.MakeDataGridSettings(DataGridSettingsVm,
                                          [nameof(ClusterResourceEx.Status),
                                           nameof(ClusterResourceEx.Type),
                                           nameof(ClusterResourceEx.Node),
                                           nameof(ClusterResourceEx.Description),
                                           nameof(ClusterResourceEx.CpuUsagePercentage),
                                           nameof(ClusterResourceEx.MemoryUsagePercentage),
                                           nameof(ClusterResourceEx.DiskUsagePercentage),
                                           nameof(ClusterResourceEx.HostCpuUsage),
                                           nameof(ClusterResourceEx.HostMemoryUsage)]);

        RadzenHelper.MakeDataGridSettings(DataGridSettingsStorage,
                                          [nameof(ClusterResourceEx.Status),
                                           nameof(ClusterResourceEx.Description),
                                           nameof(ClusterResourceEx.Node),
                                           nameof(ClusterResourceEx.PluginType),
                                           nameof(ClusterResourceEx.DiskSize),
                                           nameof(ClusterResourceEx.DiskUsage),
                                           nameof(ClusterResourceEx.DiskUsagePercentage),
                                           nameof(ClusterResourceEx.Shared),
                                           nameof(ClusterResourceEx.Content),
                                           nameof(ClusterResourceEx.Pool)]);
    }
}
