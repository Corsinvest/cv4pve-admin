/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Module.BackupAnalytics.Components;

public partial class UnprotectedGuests(IAdminService adminService) : IClusterName
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    public bool IsInitialized { get; set; }
    private DataGridSettings DataGridSettings { get; set; } = new();
    private List<long> _vmIdsInBackup = [];

    protected override void OnInitialized()
        => RadzenHelper.MakeDataGridSettings(DataGridSettings,
                                             [nameof(ClusterResourceEx.Status),
                                              nameof(ClusterResourceEx.Type),
                                              nameof(ClusterResourceEx.Node),
                                              nameof(ClusterResourceEx.Description),
                                              nameof(ClusterResourceEx.CpuUsagePercentage),
                                              nameof(ClusterResourceEx.MemoryUsagePercentage),
                                              nameof(ClusterResourceEx.DiskUsagePercentage),
                                              nameof(ClusterResourceEx.HostCpuUsage),
                                              nameof(ClusterResourceEx.HostMemoryUsage),
                                              nameof(ClusterResourceEx.Uptime)]);

    protected override async Task OnInitializedAsync()
    {
        var clusterClient = adminService[ClusterName];
        var client = await clusterClient.GetPveClientAsync();

        var backups = (await client.Cluster.Backup.GetAsync()).Where(a => a.Enabled);
        _vmIdsInBackup = !backups.Any(a => a.All)
            ? [.. backups.SelectMany(a => a.VmId.Split(",", StringSplitOptions.RemoveEmptyEntries)).Select(a => long.Parse(a))]
            : [];

        IsInitialized = true;
    }

    private bool Filter(ClusterResourceEx item, string clusterName)
        => item.ResourceType == ClusterResourceType.Vm
            && !item.IsTemplate
            && _vmIdsInBackup.Count != 0
            && !_vmIdsInBackup.Contains(item.VmId);
}
