/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json;
using Corsinvest.ProxmoxVE.Admin.Module.AIServer.Services;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Module.AIServer.Tools;

[McpServerToolType]
internal static class VmTools
{
    public enum DetailLevel
    {
        Minimal,
        Full
    }

    [McpServerTool, Description("List all virtual machines and containers in the Proxmox VE cluster")]
    public static async Task<string> ListVms([Description("Cluster name")] string cluster_name,
                                             [Description("VM status filter: running, stopped, paused (optional)")] string? status,
                                             [Description("VM type filter: qemu, lxc (optional)")] string? type,
                                             [Description("Detail level: Minimal (default) or Full (optional)")] DetailLevel detail_level,
                                             IAiServerService aiServerService)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.ListVms))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        var vms = (await clusterClient.CachedData.GetResourcesAsync(false))
                           .Where(a => a.ResourceType == ClusterResourceType.Vm
                                       && a.Status != PveConstants.StatusUnknown)
                           .Where(a => a.Status.Equals(status, StringComparison.OrdinalIgnoreCase), !string.IsNullOrEmpty(status) && !string.IsNullOrWhiteSpace(status))
                           .Where(a => a.Type.Equals(type, StringComparison.OrdinalIgnoreCase), !string.IsNullOrEmpty(type) && !string.IsNullOrWhiteSpace(type))
                           .ToList();

        vms = [.. await aiServerService.HasAsync(cluster_name, vms)];

        return detail_level == DetailLevel.Full
            ? aiServerService.SerializeTable(vms.OrderBy(a => a.VmId)
                                                .Select(a => new
                                                {
                                                    vmid = a.VmId,
                                                    name = a.Name,
                                                    node = a.Node,
                                                    status = a.Status,
                                                    type = a.Type,
                                                    description = a.Description,
                                                    is_template = a.IsTemplate,
                                                    tags = a.Tags ?? string.Empty,
                                                    uptime = a.Uptime,
                                                    @lock = a.Lock,
                                                    host_cpu_usage = a.HostCpuUsage,
                                                    host_memory_usage = a.HostMemoryUsage,
                                                    cpu_usage = a.CpuUsagePercentage,
                                                    memory_usage = a.MemoryUsage,
                                                    memory_max = a.MemorySize,
                                                    disk_usage = a.DiskUsage,
                                                    disk_max = a.DiskSize,
                                                    network_in = a.NetIn,
                                                    network_out = a.NetOut
                                                }))
            : aiServerService.SerializeTable(vms.OrderBy(a => a.VmId)
                                                .Select(a => new
                                                {
                                                    vmid = a.VmId,
                                                    name = a.Name,
                                                    node = a.Node,
                                                    status = a.Status,
                                                    type = a.Type,
                                                    tags = a.Tags ?? string.Empty,
                                                }));
    }

    [McpServerTool, Description("List VM snapshots with creation dates. Full detail includes description and parent snapshot. include_size adds size on disk in bytes (slower).")]
    public static async Task<string> ListSnapshots([Description("Cluster name")] string cluster_name,
                                                   [Description("Array of VM IDs to get snapshots for")] int[] vmids,
                                                   [Description("Detail level: Minimal (default) or Full (optional)")] DetailLevel detail_level,
                                                   [Description("Include size on disk in bytes — requires reading disk data, slower (optional, default false)")] bool include_size,
                                                   IAiServerService aiServerService)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.ListSnapshots))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        var allVms = (await clusterClient.CachedData.GetResourcesAsync(false))
                           .Where(a => a.ResourceType == ClusterResourceType.Vm)
                           .ToList();

        allVms = [.. await aiServerService.HasAsync(cluster_name, allVms)];

        var rows = new List<object>();

        var disks = include_size
            ? await clusterClient.CachedData.GetDiskSnapshotInfosAsync(false)
            : null;

        foreach (var vmid in vmids)
        {
            var vm = allVms.FirstOrDefault(a => a.VmId == vmid);
            if (vm == null) { continue; }

            var snapshots = await clusterClient.CachedData.GetSnapshotsAsync(vm.Node, vm.VmType, vm.VmId, false);

            if (detail_level == DetailLevel.Full)
            {
                rows.AddRange(snapshots.Select(s => new
                {
                    vmid = vm.VmId,
                    vm_name = vm.Name,
                    name = s.Name,
                    description = s.Description,
                    date = s.Date,
                    parent = s.Parent,
                    size = include_size
                            ? DiskInfoHelper.CalculateSnapshots(vm.VmId, s.Name, disks!)
                            : (double?)null
                }));
            }
            else
            {
                rows.AddRange(snapshots.Select(s => new
                {
                    vmid = vm.VmId,
                    vm_name = vm.Name,
                    name = s.Name,
                    date = s.Date,
                    size = include_size
                            ? DiskInfoHelper.CalculateSnapshots(vm.VmId, s.Name, disks!)
                            : (double?)null
                }));
            }
        }

        return aiServerService.SerializeTable(rows);
    }

    [McpServerTool, Description("List cluster nodes with resource usage, status and network interfaces")]
    public static async Task<string> ListNodes([Description("Cluster name")] string cluster_name,
                                               [Description("Detail level: Minimal (default) or Full (optional)")] DetailLevel detail_level,
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

        return detail_level == DetailLevel.Full
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

    [McpServerTool, Description("List cluster storage with usage statistics and type")]
    public static async Task<string> ListStorage([Description("Cluster name")] string cluster_name,
                                                 [Description("Storage type filter: dir, lvm, lvmthin, zfs, cephfs, etc. (optional)")] string? type,
                                                 [Description("Detail level: Minimal (default) or Full (optional)")] DetailLevel detail_level,
                                                 IAiServerService aiServerService)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.ListStorage))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        var storages = (await clusterClient.CachedData.GetResourcesAsync(false))
                           .Where(a => a.ResourceType == ClusterResourceType.Storage
                                       && a.Status != PveConstants.StatusUnknown)
                           .Where(a => a.Type.Equals(type, StringComparison.OrdinalIgnoreCase), !string.IsNullOrEmpty(type) && !string.IsNullOrWhiteSpace(type))
                           .ToList();

        storages = [.. await aiServerService.HasAsync(cluster_name, storages)];

        return detail_level == DetailLevel.Full
            ? aiServerService.SerializeTable(storages.OrderBy(a => a.Storage)
                                                     .Select(a => new
                                                     {
                                                         storage = a.Storage,
                                                         node = a.Node,
                                                         type = a.Type,
                                                         plugin_type = a.PluginType,
                                                         status = a.Status,
                                                         content = a.Content,
                                                         shared = a.Shared,
                                                         is_available = a.IsAvailable,
                                                         disk_usage = a.DiskUsage,
                                                         disk_total = a.DiskSize,
                                                         disk_usage_percentage = a.DiskUsagePercentage
                                                     }))
            : aiServerService.SerializeTable(storages.OrderBy(a => a.Storage)
                                                     .Select(a => new
                                                     {
                                                         storage = a.Storage,
                                                         node = a.Node,
                                                         type = a.Type,
                                                         status = a.Status,
                                                         disk_usage_percentage = a.DiskUsagePercentage
                                                     }));
    }

    [McpServerTool, Description("List cluster pools with descriptions")]
    public static async Task<string> ListPools([Description("Cluster name")] string cluster_name,
                                               IAiServerService aiServerService)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.ListPools))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        var pools = (await clusterClient.CachedData.GetResourcesAsync(false))
                           .Where(a => a.ResourceType == ClusterResourceType.Pool)
                           .ToList();

        pools = [.. await aiServerService.HasAsync(cluster_name, pools)];

        return aiServerService.SerializeTable(pools.OrderBy(a => a.Pool)
                                                   .Select(a => new
                                                   {
                                                       poolid = a.Pool,
                                                       description = a.Description
                                                   }));
    }
}
