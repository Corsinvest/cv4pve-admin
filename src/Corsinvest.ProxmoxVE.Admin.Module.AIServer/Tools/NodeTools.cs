/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json;
using Corsinvest.ProxmoxVE.Admin.Module.AIServer.Services;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Module.AIServer.Tools;

[McpServerToolType]
internal static class NodeTools
{
    [McpServerTool, Description("List cluster nodes with resource usage, status and network interfaces")]
    public static async Task<string> ListNodes([Description("Cluster name")] string cluster_name,
                                               [Description("Detail level: Minimal (default) or Full (optional)")] VmTools.DetailLevel detail_level,
                                               IAiServerService aiServerService)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.ListNodes))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        var nodes = (await clusterClient.CachedData.GetResourcesAsync(false))
                           .Where(a => a.ResourceType == ClusterResourceType.Node
                                       && a.Status != PveConstants.StatusUnknown)
                           .ToList();

        nodes = [.. await aiServerService.HasAsync(cluster_name, nodes)];

        return detail_level == VmTools.DetailLevel.Full
            ? aiServerService.SerializeTable(nodes.OrderBy(a => a.Node)
                                                  .Select(a => new
                                                  {
                                                      node = a.Node,
                                                      status = a.Status,
                                                      uptime = a.Uptime,
                                                      cpu_usage = a.CpuUsagePercentage,
                                                      cpu_total = a.CpuSize,
                                                      memory_usage = a.MemoryUsage,
                                                      memory_total = a.MemorySize,
                                                      memory_usage_percentage = a.MemoryUsagePercentage,
                                                      disk_usage = a.DiskUsage,
                                                      disk_total = a.DiskSize,
                                                      disk_usage_percentage = a.DiskUsagePercentage,
                                                      level = a.NodeLevel,
                                                      network_in = a.NetIn,
                                                      network_out = a.NetOut
                                                  }))
            : aiServerService.SerializeTable(nodes.OrderBy(a => a.Node)
                                                  .Select(a => new
                                                  {
                                                      node = a.Node,
                                                      status = a.Status,
                                                      cpu_usage = a.CpuUsagePercentage,
                                                      memory_usage_percentage = a.MemoryUsagePercentage,
                                                      disk_usage_percentage = a.DiskUsagePercentage
                                                  }));
    }

    [McpServerTool, Description("Get detailed status of a single cluster node (CPU model, kernel version, PVE version, memory, swap, load average, root fs)")]
    public static async Task<string> GetNodeStatus([Description("Cluster name")] string cluster_name,
                                                   [Description("Node name")] string node,
                                                   IAiServerService aiServerService)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.GetNodeStatus))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        var nodeResource = (await clusterClient.CachedData.GetResourcesAsync(false))
                               .FirstOrDefault(a => a.ResourceType == ClusterResourceType.Node
                                                    && a.Node.Equals(node, StringComparison.OrdinalIgnoreCase));

        if (nodeResource == null) { return JsonSerializer.Serialize(new { error = $"Node '{node}' not found" }); }

        if (!(await aiServerService.HasAsync(cluster_name, [nodeResource])).Any())
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var pveClient = await clusterClient.GetPveClientAsync();
        var status = await pveClient.Nodes[node].Status.GetAsync();

        if (status == null) { return JsonSerializer.Serialize(new { error = $"Node '{node}' not found or unreachable" }); }

        var loadAvg = status.LoadAvg.ToArray();
        return JsonSerializer.Serialize(new
        {
            node,
            pve_version = status.PveVersion,
            kernel_version = status.CurrentKernel?.Release,
            kernel_sysname = status.CurrentKernel?.Sysname,
            uptime = status.Uptime,
            cpu_usage = status.Cpu,
            load_avg_1m = loadAvg.Length > 0 ? loadAvg[0] : null,
            load_avg_5m = loadAvg.Length > 1 ? loadAvg[1] : null,
            load_avg_15m = loadAvg.Length > 2 ? loadAvg[2] : null,
            cpu_model = status.CpuInfo?.Model,
            cpu_sockets = status.CpuInfo?.Sockets,
            cpu_cores = status.CpuInfo?.Cores,
            cpu_mhz = status.CpuInfo?.Mhz,
            memory_used = status.Memory?.Used,
            memory_total = status.Memory?.Total,
            memory_free = status.Memory?.Free,
            swap_used = status.Swap?.Used,
            swap_total = status.Swap?.Total,
            rootfs_used = status.RootFs?.Used,
            rootfs_total = status.RootFs?.Total,
            rootfs_avail = status.RootFs?.Available,
            boot_mode = status.BootInfo?.Mode,
            secure_boot = status.BootInfo?.Secureboot,
            io_delay = status.Wait
        });
    }

    [McpServerTool, Description("List cluster replications with status and schedule")]
    public static async Task<string> ListReplications([Description("Cluster name")] string cluster_name,
                                                      IAiServerService aiServerService)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.ListReplications))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        var nodes = (await clusterClient.CachedData.GetResourcesAsync(false))
                           .Where(a => a.ResourceType == ClusterResourceType.Node)
                           .ToList();

        nodes = [.. await aiServerService.HasAsync(cluster_name, nodes)];

        var allReplications = await Task.WhenAll(nodes.Select(node =>
            clusterClient.CachedData.GetReplicationsAsync(node.Node, null, false)));

        return aiServerService.SerializeTable(allReplications.SelectMany(a => a)
                                                             .Select(a => new
                                                             {
                                                                 id = a.Id,
                                                                 vmid = a.Guest,
                                                                 source = a.Source,
                                                                 target = a.Target,
                                                                 type = a.Type,
                                                                 disabled = a.Disable,
                                                                 schedule = a.Schedule,
                                                                 last_sync = a.LastSync,
                                                                 next_sync = a.NextSync,
                                                                 duration = a.Duration,
                                                                 fail_count = a.FailCount,
                                                                 error = a.Error
                                                             })
                                                             .OrderBy(a => a.id));
    }

    [McpServerTool, Description("Get historical metrics data (CPU, Memory, Network, etc.) for cluster nodes")]
    public static async Task<string> ListNodeRrdData([Description("Cluster name")] string cluster_name,
                                                     [Description("Array of node names to get RRD data for (optional, all nodes if empty)")] string[]? nodes,
                                                     [Description("Time frame: Hour, Day (default), Week, Month, Year (optional)")] RrdDataTimeFrame time_frame,
                                                     [Description("Consolidation method: Average (default), Maximum (optional)")] RrdDataConsolidation consolidation,
                                                     [Description("Metric categories: Cpu (LoadAvg,iowait), Memory (Swap), Disk, Network, Pressure (PSI, PVE 9.0+), All (default) (optional)")] VmTools.RrdMetricCategory metrics,
                                                     IAiServerService aiServerService)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.ListNodeRrdData))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        var allNodes = (await clusterClient.CachedData.GetResourcesAsync(false))
                           .Where(a => a.ResourceType == ClusterResourceType.Node
                                       && a.Status != PveConstants.StatusUnknown)
                           .ToList();

        allNodes = [.. await aiServerService.HasAsync(cluster_name, allNodes)];

        var targetNodes = nodes?.Length > 0
            ? [.. allNodes.Where(n => nodes.Contains(n.Node, StringComparer.OrdinalIgnoreCase))]
            : allNodes;

        var results = new List<object>();

        foreach (var node in targetNodes)
        {
            var rrdData = await clusterClient.CachedData.GetRrdDataAsync(node.Node, time_frame, consolidation, false);

            results.Add(new
            {
                node = node.Node,
                data = rrdData.Select(d =>
                {
                    var obj = new Dictionary<string, object> { { "time", d.TimeDate } };

                    if (metrics.HasFlag(VmTools.RrdMetricCategory.Cpu))
                    {
                        obj["cpu_usage"] = d.CpuUsagePercentage;
                        obj["cpu_total"] = d.CpuSize;
                        obj["loadavg"] = d.Loadavg;
                        obj["iowait"] = d.IoWait;
                    }

                    if (metrics.HasFlag(VmTools.RrdMetricCategory.Memory))
                    {
                        obj["memory_usage"] = d.MemoryUsage;
                        obj["memory_total"] = d.MemorySize;
                        obj["swap_usage"] = d.SwapUsage;
                        obj["swap_total"] = d.SwapSize;
                    }

                    if (metrics.HasFlag(VmTools.RrdMetricCategory.Disk))
                    {
                        obj["root_usage"] = d.RootUsage;
                        obj["root_total"] = d.RootSize;
                    }

                    if (metrics.HasFlag(VmTools.RrdMetricCategory.Network))
                    {
                        obj["network_in"] = d.NetIn;
                        obj["network_out"] = d.NetOut;
                    }

                    if (metrics.HasFlag(VmTools.RrdMetricCategory.Pressure))
                    {
                        obj["pressure_cpu_some"] = d.PressureCpuSome;
                        obj["pressure_io_some"] = d.PressureIoSome;
                        obj["pressure_io_full"] = d.PressureIoFull;
                        obj["pressure_memory_some"] = d.PressureMemorySome;
                        obj["pressure_memory_full"] = d.PressureMemoryFull;
                    }

                    return obj;
                }).ToArray()
            });
        }

        return JsonSerializer.Serialize(new { nodes = results });
    }

    [McpServerTool, Description("List cluster tasks (active and recent)")]
    public static async Task<string> ListTasks([Description("Cluster name")] string cluster_name,
                                               [Description("Status filter: running, stopped (optional, all if omitted)")] string? status,
                                               [Description("Task type filter: vzdump, qmstart, qmstop, etc. (optional)")] string? type,
                                               [Description("Maximum number of tasks to return per node (default: 50)")] int limit,
                                               IAiServerService aiServerService)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.ListTasks))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        if (limit <= 0) { limit = 50; }
        var client = await clusterClient.GetPveClientAsync();

        var nodes = (await clusterClient.CachedData.GetResourcesAsync(false))
                           .Where(a => a.ResourceType == ClusterResourceType.Node
                                       && a.Status != PveConstants.StatusUnknown)
                           .ToList();

        nodes = [.. await aiServerService.HasAsync(cluster_name, nodes)];

        var allTasks = new List<(DateTime StartTime, object Row)>();

        foreach (var node in nodes)
        {
            var tasks = await client.Nodes[node.Node].Tasks.GetAsync(limit: limit, typefilter: type);
            allTasks.AddRange(tasks
                .Where(t => string.IsNullOrEmpty(status) || t.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                .Select(t => (t.StartTimeDate, (object)new
                {
                    upid = t.UniqueTaskId,
                    node = t.Node,
                    type = t.Type,
                    description = t.Description,
                    status = t.Status,
                    user = t.User,
                    start_time = t.StartTimeDate,
                    end_time = t.EndTime > 0 ? t.EndTimeDate : (DateTime?)null,
                    duration = t.DurationInfo
                })));
        }

        return aiServerService.SerializeTable(allTasks.OrderByDescending(t => t.StartTime).Take(limit).Select(t => t.Row));
    }
}
