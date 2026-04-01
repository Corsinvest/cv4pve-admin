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
                                          [nameof(ClusterResourceItem.Status),
                                           nameof(ClusterResourceItem.Type),
                                           nameof(ClusterResourceItem.Node),
                                           nameof(ClusterResourceItem.Description),
                                           nameof(ClusterResourceItem.CpuUsagePercentage),
                                           nameof(ClusterResourceItem.MemoryUsagePercentage),
                                           nameof(ClusterResourceItem.DiskUsagePercentage),
                                           nameof(ClusterResourceItem.HostCpuUsage),
                                           nameof(ClusterResourceItem.HostMemoryUsage),
                                           nameof(ClusterResourceItem.HealthScore)]);

        RadzenHelper.MakeDataGridSettings(DataGridSettingsStorage,
                                          [nameof(ClusterResourceItem.Status),
                                           nameof(ClusterResourceItem.Description),
                                           nameof(ClusterResourceItem.Node),
                                           nameof(ClusterResourceItem.PluginType),
                                           nameof(ClusterResourceItem.DiskSize),
                                           nameof(ClusterResourceItem.DiskUsage),
                                           nameof(ClusterResourceItem.DiskUsagePercentage),
                                           nameof(ClusterResourceItem.Shared),
                                           nameof(ClusterResourceItem.Content),
                                           nameof(ClusterResourceItem.HealthScore),
                                           nameof(ClusterResourceItem.Pool)]);
    }
}
