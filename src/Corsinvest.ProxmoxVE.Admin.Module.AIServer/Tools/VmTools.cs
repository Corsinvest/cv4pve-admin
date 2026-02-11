using System.Text.Json;
using Corsinvest.ProxmoxVE.Admin.Module.AIServer.Helpers;
using Microsoft.AspNetCore.Http;
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

    // private static IEnumerable<ClusterResource> Filter(IPermissionService permissionService,
    //                                                    IEnumerable<ClusterResource> items,
    //                                                    string clsuterName)
    // {
    //     var data= items.ToList();
    //     foreach (var item in data.ToArray())
    //     {
    //         if (!await permissionService.HasVmAsync(clsuterName, QueryVm, item.vmid))
    //         {
    //             data.Remove(item);
    //         }
    //     }
    //     return data;
    // }

    [McpServerTool, Description("List all virtual machines and containers in the Proxmox VE cluster")]
    public static async Task<string> ListVms([Description("VM status filter: running, stopped, paused (optional)")] string? status,
                                             [Description("VM type filter: qemu, lxc (optional)")] string? type,
                                             [Description("Detail level: Minimal (default) or Full (optional)")] DetailLevel? detail_level,
                                             IAdminService adminService,
                                             //IPermissionService permissionService,
                                             IHttpContextAccessor httpContextAccessor)
    {
        // Get cluster client from context with validation
        var (clusterClient, errorJson) = McpToolsHelper.GetClusterClient(httpContextAccessor, adminService);
        if (clusterClient == null) { return errorJson!; }

        var vms = (await clusterClient.CachedData.GetResourcesAsync(false))
                           .Where(a => a.ResourceType == ClusterResourceType.Vm
                                       && a.Status != PveConstants.StatusUnknown)
                           .Where(a => a.Status.Equals(status, StringComparison.OrdinalIgnoreCase), !string.IsNullOrEmpty(status) && !string.IsNullOrWhiteSpace(status))
                           .Where(a => a.Type.Equals(type, StringComparison.OrdinalIgnoreCase), !string.IsNullOrEmpty(type) && !string.IsNullOrWhiteSpace(type));

        var detailLevel = detail_level ?? DetailLevel.Minimal;
        object results;

        if (detailLevel == DetailLevel.Full)
        {
            results = vms.Select(vm => new
            {
                vmid = vm.VmId,
                name = vm.Name,
                node = vm.Node,
                status = vm.Status,
                type = vm.Type,
                description = vm.Description,
                is_template = vm.IsTemplate,
                tags = (vm.Tags + "").Split(';'),
                uptime = vm.Uptime,
                @lock = vm.Lock,
                host_cpu_usage = vm.HostCpuUsage,
                host_memory_usage = vm.HostMemoryUsage,
                cpu_usage = vm.CpuUsagePercentage,
                memory_usage = vm.MemoryUsage,
                memory_max = vm.MemorySize,
                disk_usage = vm.DiskUsage,
                disk_max = vm.DiskSize,
                network_in = vm.NetIn,
                network_out = vm.NetOut
            })
            .OrderBy(v => v.vmid)
            .ToList();
        }
        else
        {
            results = vms.Select(vm => new
            {
                vmid = vm.VmId,
                name = vm.Name,
                node = vm.Node,
                status = vm.Status,
                type = vm.Type
            })
            .OrderBy(v => v.vmid)
            .ToList();
        }

        return JsonSerializer.Serialize(new
        {
            cluster_name = clusterClient.Settings.Name,
            total_vms = vms.Count(),
            detail_level = detailLevel.ToString().ToLower(),
            vms = results
        });
    }

    [McpServerTool, Description("List VM snapshots with creation dates and descriptions")]
    public static async Task<string> ListSnapshots([Description("Array of VM IDs to get snapshots for")] int[] vmids,
                                                   [Description("Detail level: Minimal (default) or Full (optional)")] DetailLevel? detail_level,
                                                   IAdminService adminService,
                                                   //IPermissionService permissionService,
                                                   IHttpContextAccessor httpContextAccessor)
    {
        // Get cluster client from context with validation
        var (clusterClient, errorJson) = McpToolsHelper.GetClusterClient(httpContextAccessor, adminService);
        if (clusterClient == null) { return errorJson!; }

        var allVms = (await clusterClient.CachedData.GetResourcesAsync(false))
                           .Where(a => a.ResourceType == ClusterResourceType.Vm)
                           .ToList();

        var detailLevel = detail_level ?? DetailLevel.Minimal;
        var results = new List<object>();

        foreach (var vmid in vmids)
        {
            var vm = allVms.FirstOrDefault(a => a.VmId == vmid);

            if (vm == null)
            {
                results.Add(new
                {
                    vmid,
                    error = $"VM with ID {vmid} not found"
                });
                continue;
            }

            var snapshots = await clusterClient.CachedData.GetSnapshotsAsync(vm.Node, vm.VmType, vm.VmId, false);

            object snapshotData;
            if (detailLevel == DetailLevel.Full)
            {
                snapshotData = snapshots.Select(s => new
                {
                    name = s.Name,
                    description = s.Description,
                    date = s.Date,
                    parent = s.Parent
                });
            }
            else
            {
                snapshotData = snapshots.Select(s => new
                {
                    name = s.Name,
                    date = s.Date
                });
            }

            results.Add(new
            {
                vmid = vm.VmId,
                name = vm.Name,
                total_snapshots = snapshots.Count(),
                snapshots = snapshotData
            });
        }

        return JsonSerializer.Serialize(new
        {
            cluster_name = clusterClient.Settings.Name,
            total_vms = results.Count,
            detail_level = detailLevel.ToString().ToLower(),
            vms = results
        });
    }

    [McpServerTool, Description("List cluster nodes with resource usage, status and network interfaces")]
    public static async Task<string> ListNodes([Description("Detail level: Minimal (default) or Full (optional)")] DetailLevel? detail_level,
                                               IAdminService adminService,
                                               IHttpContextAccessor httpContextAccessor)
    {
        // Get cluster client from context with validation
        var (clusterClient, errorJson) = McpToolsHelper.GetClusterClient(httpContextAccessor, adminService);
        if (clusterClient == null) { return errorJson!; }

        var nodes = (await clusterClient.CachedData.GetResourcesAsync(false))
                           .Where(a => a.ResourceType == ClusterResourceType.Node
                                       && a.Status != PveConstants.StatusUnknown)
                           .ToList();

        var detailLevel = detail_level ?? DetailLevel.Minimal;
        var results = new List<object>();

        foreach (var node in nodes)
        {
            if (detailLevel == DetailLevel.Full)
            {
                results.Add(new
                {
                    node = node.Node,
                    status = node.Status,
                    uptime = node.Uptime,
                    cpu_usage = node.CpuUsagePercentage,
                    cpu_total = node.CpuSize,
                    memory_usage = node.MemoryUsage,
                    memory_total = node.MemorySize,
                    memory_usage_percentage = node.MemoryUsagePercentage,
                    disk_usage = node.DiskUsage,
                    disk_total = node.DiskSize,
                    disk_usage_percentage = node.DiskUsagePercentage,
                    level = node.NodeLevel,
                    network_int = node.NetIn,
                    network_out = node.NetOut
                });
            }
            else
            {
                results.Add(new
                {
                    node = node.Node,
                    status = node.Status,
                    cpu_usage = node.CpuUsagePercentage,
                    memory_usage_percentage = node.MemoryUsagePercentage,
                    disk_usage_percentage = node.DiskUsagePercentage
                });
            }
        }

        return JsonSerializer.Serialize(new
        {
            cluster_name = clusterClient.Settings.Name,
            total_nodes = results.Count,
            detail_level = detailLevel.ToString().ToLower(),
            nodes = results.OrderBy(n => ((dynamic)n).node)
        });
    }

    [McpServerTool, Description("List cluster storage with usage statistics and type")]
    public static async Task<string> ListStorage([Description("Storage type filter: dir, lvm, lvmthin, zfs, cephfs, etc. (optional)")] string? type,
                                                 [Description("Detail level: Minimal (default) or Full (optional)")] DetailLevel? detail_level,
                                                 IAdminService adminService,
                                                 IHttpContextAccessor httpContextAccessor)
    {
        // Get cluster client from context with validation
        var (clusterClient, errorJson) = McpToolsHelper.GetClusterClient(httpContextAccessor, adminService);
        if (clusterClient == null) { return errorJson!; }

        var storages = (await clusterClient.CachedData.GetResourcesAsync(false))
                           .Where(a => a.ResourceType == ClusterResourceType.Storage
                                       && a.Status != PveConstants.StatusUnknown)
                           .Where(a => a.Type.Equals(type, StringComparison.OrdinalIgnoreCase), !string.IsNullOrEmpty(type) && !string.IsNullOrWhiteSpace(type));

        var detailLevel = detail_level ?? DetailLevel.Minimal;
        object results;

        if (detailLevel == DetailLevel.Full)
        {
            results = storages.Select(storage => new
            {
                storage = storage.Storage,
                node = storage.Node,
                type = storage.Type,
                plugin_type = storage.PluginType,
                status = storage.Status,
                content = storage.Content,
                shared = storage.Shared,
                is_available = storage.IsAvailable,
                disk_usage = storage.DiskUsage,
                disk_total = storage.DiskSize,
                disk_usage_percentage = storage.DiskUsagePercentage
            })
            .OrderBy(s => s.storage)
            .ToList();
        }
        else
        {
            results = storages.Select(storage => new
            {
                storage = storage.Storage,
                node = storage.Node,
                type = storage.Type,
                status = storage.Status,
                disk_usage_percentage = storage.DiskUsagePercentage
            })
            .OrderBy(s => s.storage)
            .ToList();
        }

        return JsonSerializer.Serialize(new
        {
            cluster_name = clusterClient.Settings.Name,
            total_storages = storages.Count(),
            detail_level = detailLevel.ToString().ToLower(),
            storages = results
        });
    }

    [McpServerTool, Description("List cluster pools with descriptions")]
    public static async Task<string> ListPools(IAdminService adminService,
                                               IHttpContextAccessor httpContextAccessor)
    {
        // Get cluster client from context with validation
        var (clusterClient, errorJson) = McpToolsHelper.GetClusterClient(httpContextAccessor, adminService);
        if (clusterClient == null) { return errorJson!; }

        var pools = (await clusterClient.CachedData.GetResourcesAsync(false))
                           .Where(a => a.ResourceType == ClusterResourceType.Pool)
                           .ToList();

        var results = pools.Select(pool => new
        {
            poolid = pool.Pool,
            description = pool.Description
        })
        .OrderBy(p => p.poolid)
        .ToList();

        return JsonSerializer.Serialize(new
        {
            cluster_name = clusterClient.Settings.Name,
            total_pools = results.Count,
            pools = results
        });
    }

}
