/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Cluster;
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Microsoft.AspNetCore.WebUtilities;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components;

public partial class Vms : IClusterName
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private ResourcesEx? ResourcesExRef { get; set; }
    private bool ShowOsInfo { get; set; }
    private DataGridSettings DataGridSettings { get; set; } = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) { return; }

        var query = QueryHelpers.ParseQuery(new Uri(NavigationManager.Uri).Query);
        if (query.TryGetValue("vmid", out var vmIdStr) && long.TryParse(vmIdStr, out var vmId))
        {
            await ResourcesExRef!.ExpandRowsAsync(ResourcesExRef.GetItems().Where(a => a.VmId == vmId));
        }
    }

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
                                              nameof(ClusterResourceEx.HealthScore),
                                              nameof(ClusterResourceEx.Uptime)]);

    private async Task ShowOsInfoAfter()
    {
        DataGridSettings = RadzenHelper.MakeDataGridSettings([nameof(ClusterResourceEx.Status),
                                                              nameof(ClusterResourceEx.Type),
                                                              nameof(ClusterResourceEx.Node),
                                                              nameof(ClusterResourceEx.Description),
                                                              nameof(ClusterResourceEx.CpuUsagePercentage),
                                                              nameof(ClusterResourceEx.MemoryUsagePercentage),
                                                              nameof(ClusterResourceEx.DiskUsagePercentage),
                                                              nameof(ClusterResourceEx.HostCpuUsage),
                                                              nameof(ClusterResourceEx.HostMemoryUsage),
                                                              nameof(ClusterResourceEx.Uptime),
                                                              nameof(ClusterResourceEx.OsIcon),
                                                              nameof(ClusterResourceEx.HostName),
                                                              nameof(ClusterResourceEx.OsVersion),
                                                              nameof(ClusterResourceEx.OsType)]);

        await ResourcesExRef!.ReloadSettingsAsync();
        await ResourcesExRef!.RefreshDataAsync();
    }
}
