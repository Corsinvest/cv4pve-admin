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
    private readonly List<BackupJob> _jobs = [];

    private record BackupJob(bool All, HashSet<long> VmIds, HashSet<long> Exclude, string? Pool, string? Node);

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
                                              nameof(ClusterResourceItem.Uptime)]);

    protected override async Task OnInitializedAsync()
    {
        var clusterClient = adminService[ClusterName];
        var client = await clusterClient.GetPveClientAsync();

        // As returned by Proxmox VE: the list of the excluded guests is not in the model of the library
        var result = await client.Cluster.Backup.Index();
        if (result.IsSuccessStatusCode && result.Response.data is IEnumerable<object> items)
        {
            foreach (var job in items.OfType<IDictionary<string, object>>().Where(a => IsOn(a, "enabled", true)))
            {
                _jobs.Add(new(IsOn(job, "all", false),
                              GetIds(job, "vmid"),
                              GetIds(job, "exclude"),
                              GetText(job, "pool"),
                              GetText(job, "node")));
            }
        }

        IsInitialized = true;
    }

    private static string? GetText(IDictionary<string, object> job, string key)
        => job.TryGetValue(key, out var value) ? value?.ToString() : null;

    private static bool IsOn(IDictionary<string, object> job, string key, bool whenMissing)
        => GetText(job, key) is { Length: > 0 } value
            ? value is "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase)
            : whenMissing;

    private static HashSet<long> GetIds(IDictionary<string, object> job, string key)
        => [.. (GetText(job, key) ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                                 .Where(a => long.TryParse(a, out _))
                                                 .Select(long.Parse)];

    // A job covers every guest but the excluded ones, or the guests of a pool, or the guests it lists;
    // when it is bound to a node, only the guests on that node
    private bool IsProtected(ClusterResourceItem item)
        => _jobs.Any(a => (string.IsNullOrEmpty(a.Node) || a.Node == item.Node)
                          && (a.All
                                ? !a.Exclude.Contains(item.VmId)
                                : !string.IsNullOrEmpty(a.Pool)
                                    ? a.Pool == item.Pool
                                    : a.VmIds.Contains(item.VmId)));

    private bool FilterExpression(ClusterResourceItem item, string clusterName)
        => item.ClusterName == clusterName
            && item.ResourceType == ClusterResourceType.Vm
            && !item.IsTemplate
            && !IsProtected(item);
}
