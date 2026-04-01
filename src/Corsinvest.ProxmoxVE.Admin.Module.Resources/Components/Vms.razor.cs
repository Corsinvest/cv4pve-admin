/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components;

public partial class Vms : IClusterName
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;
    [SupplyParameterFromQuery] public int? VmId { get; set; }

    private ResourcesView? ResourcesExRef { get; set; }
    private bool ShowOsInfo { get; set; }
    private DataGridSettings DataGridSettings { get; set; } = new();

    private bool _expanded;
    private async Task OnDataLoadedAsync()
    {
        if (_expanded) { return; }
        if (VmId != null)
        {
            _expanded = true;
            await ResourcesExRef!.ExpandRowsAsync(ResourcesExRef.GetItems().Where(a => a.VmId == VmId));
        }
    }

    protected override void OnInitialized()
        => RadzenHelper.MakeDataGridSettings(DataGridSettings,
                                             [nameof(ClusterResourceItem.Status),
                                              nameof(ClusterResourceItem.Type),
                                              nameof(ClusterResourceItem.Node),
                                              nameof(ClusterResourceItem.Description),
                                              nameof(ClusterResourceItem.CpuUsagePercentage),
                                              nameof(ClusterResourceItem.MemoryUsagePercentage),
                                              nameof(ClusterResourceItem.DiskUsagePercentage),
                                              nameof(ClusterResourceItem.HostCpuUsage),
                                              nameof(ClusterResourceItem.HostMemoryUsage),
                                              nameof(ClusterResourceItem.HealthScore),
                                              nameof(ClusterResourceItem.Uptime)]);

    private async Task ShowOsInfoAfter()
    {
        DataGridSettings = RadzenHelper.MakeDataGridSettings([nameof(ClusterResourceItem.Status),
                                                              nameof(ClusterResourceItem.Type),
                                                              nameof(ClusterResourceItem.Node),
                                                              nameof(ClusterResourceItem.Description),
                                                              nameof(ClusterResourceItem.CpuUsagePercentage),
                                                              nameof(ClusterResourceItem.MemoryUsagePercentage),
                                                              nameof(ClusterResourceItem.DiskUsagePercentage),
                                                              nameof(ClusterResourceItem.HostCpuUsage),
                                                              nameof(ClusterResourceItem.HostMemoryUsage),
                                                              nameof(ClusterResourceItem.Uptime),
                                                              nameof(ClusterResourceItem.OsIcon),
                                                              nameof(ClusterResourceItem.HostName),
                                                              nameof(ClusterResourceItem.OsVersion),
                                                              nameof(ClusterResourceItem.OsType)]);

        await ResourcesExRef!.ReloadSettingsAsync();
        await ResourcesExRef!.RefreshDataAsync();
    }
}
