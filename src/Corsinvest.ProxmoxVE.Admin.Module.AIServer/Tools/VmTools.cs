/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json;
using Corsinvest.ProxmoxVE.Admin.Core.Commands;
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Vm;
using Corsinvest.ProxmoxVE.Admin.Module.AIServer.Services;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
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

    [Flags]
    public enum RrdMetricCategory
    {
        None = 0,
        Cpu = 1,
        Memory = 2,
        Disk = 4,
        Network = 8,
        Pressure = 16,
        All = Cpu | Memory | Disk | Network | Pressure
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

    [McpServerTool, Description("Get detailed configuration of a VM or LXC container (CPU, memory, disks, network, agent, boot options)")]
    public static async Task<string> GetVmConfig([Description("Cluster name")] string cluster_name,
                                                 [Description("VM ID")] int vmid,
                                                 IAiServerService aiServerService)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.GetVmConfig))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        var vm = (await clusterClient.CachedData.GetResourcesAsync(false))
                      .FirstOrDefault(a => a.ResourceType == ClusterResourceType.Vm && a.VmId == vmid);

        if (vm == null) { return JsonSerializer.Serialize(new { error = $"VM {vmid} not found" }); }

        if (!(await aiServerService.HasAsync(cluster_name, [vm])).Any())
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        if (vm.VmType == VmType.Qemu)
        {
            var config = await clusterClient.CachedData.GetQemuConfigAsync(vm.Node, vmid, false);
            return JsonSerializer.Serialize(new
            {
                vmid,
                name = vm.Name,
                node = vm.Node,
                type = "qemu",
                cores = config.Cores,
                sockets = config.Sockets,
                cpu = config.Cpu,
                arch = config.Arch,
                memory_mb = config.Memory,
                balloon_mb = config.Balloon,
                bios = config.Bios,
                os_type = config.OsType,
                kvm = config.Kvm,
                agent = config.Agent,
                agent_enabled = config.AgentEnabled,
                on_boot = config.OnBoot,
                startup = config.StartUp,
                protection = config.Protection,
                acpi = config.Acpi,
                description = config.Description,
                tags = config.Tags
            });
        }
        else
        {
            var config = await clusterClient.CachedData.GetLxcConfigAsync(vm.Node, vmid, false);
            return JsonSerializer.Serialize(new
            {
                vmid,
                name = vm.Name,
                node = vm.Node,
                type = "lxc",
                cores = config.Cores,
                arch = config.Arch,
                memory_mb = config.Memory,
                swap_mb = config.Swap,
                os_type = config.OsType,
                on_boot = config.OnBoot,
                startup = config.Startup,
                protection = config.Protection,
                description = config.Description,
                tags = config.Tags
            });
        }
    }

    [McpServerTool, Description("Change the power state of a VM or LXC container. Actions: Start, Stop (force), Shutdown (graceful), Reset")]
    public static async Task<string> ChangeVmState([Description("Cluster name")] string cluster_name,
                                                   [Description("VM ID")] int vmid,
                                                   [Description("Action: Start, Stop, Shutdown, Reset")] VmStatus action,
                                                   IAiServerService aiServerService,
                                                   IPermissionService permissionService,
                                                   CommandExecutor commandExecutor)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.ChangeVmState))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        var vm = (await clusterClient.CachedData.GetResourcesAsync(false))
                      .FirstOrDefault(a => a.ResourceType == ClusterResourceType.Vm && a.VmId == vmid);

        if (vm == null) { return JsonSerializer.Serialize(new { error = $"VM {vmid} not found" }); }

        if (!await permissionService.HasVmAsync(cluster_name, ClusterPermissions.Vm.PowerManagement, vmid))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var result = await commandExecutor.ExecuteAsync(new VmChangeStateCommand(cluster_name, vmid, action));

        return result.IsSuccess
            ? JsonSerializer.Serialize(new { success = true, vmid, action = action.ToString(), upid = result.Upid })
            : JsonSerializer.Serialize(new { error = result.ErrorMessage });
    }

    [McpServerTool, Description("Create a snapshot of a VM or LXC container")]
    public static async Task<string> CreateVmSnapshot([Description("Cluster name")] string cluster_name,
                                                      [Description("VM ID")] int vmid,
                                                      [Description("Snapshot name")] string snapshot_name,
                                                      [Description("Snapshot description (optional)")] string? description,
                                                      [Description("Include VM RAM state — QEMU only (optional, default false)")] bool include_vm_state,
                                                      IAiServerService aiServerService,
                                                      IPermissionService permissionService,
                                                      CommandExecutor commandExecutor)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.CreateVmSnapshot))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        if (!await permissionService.HasVmAsync(cluster_name, ClusterPermissions.Vm.Snapshot, vmid))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var result = await commandExecutor.ExecuteAsync(new VmCreateSnapshotCommand(cluster_name, vmid, snapshot_name, description, include_vm_state));

        return result.IsSuccess
            ? JsonSerializer.Serialize(new { success = true, vmid, snapshot_name, upid = result.Upid })
            : JsonSerializer.Serialize(new { error = result.ErrorMessage });
    }

    [McpServerTool, Description("Delete a snapshot of a VM or LXC container")]
    public static async Task<string> DeleteVmSnapshot([Description("Cluster name")] string cluster_name,
                                                      [Description("VM ID")] int vmid,
                                                      [Description("Snapshot name")] string snapshot_name,
                                                      IAiServerService aiServerService,
                                                      IPermissionService permissionService,
                                                      CommandExecutor commandExecutor)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.DeleteVmSnapshot))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        if (!await permissionService.HasVmAsync(cluster_name, ClusterPermissions.Vm.Snapshot, vmid))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var result = await commandExecutor.ExecuteAsync(new VmRemoveSnapshotCommand(cluster_name, vmid, snapshot_name));

        return result.IsSuccess
            ? JsonSerializer.Serialize(new { success = true, vmid, snapshot_name, upid = result.Upid })
            : JsonSerializer.Serialize(new { error = result.ErrorMessage });
    }

    [McpServerTool, Description("Rollback a VM or LXC container to a snapshot")]
    public static async Task<string> RollbackVmSnapshot([Description("Cluster name")] string cluster_name,
                                                        [Description("VM ID")] int vmid,
                                                        [Description("Snapshot name")] string snapshot_name,
                                                        IAiServerService aiServerService,
                                                        IPermissionService permissionService,
                                                        CommandExecutor commandExecutor)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.RollbackVmSnapshot))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        if (!await permissionService.HasVmAsync(cluster_name, ClusterPermissions.Vm.SnapshotRallback, vmid))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var result = await commandExecutor.ExecuteAsync(new VmRollbackSnapshotCommand(cluster_name, vmid, snapshot_name));

        return result.IsSuccess
            ? JsonSerializer.Serialize(new { success = true, vmid, snapshot_name, upid = result.Upid })
            : JsonSerializer.Serialize(new { error = result.ErrorMessage });
    }

    [McpServerTool, Description("Migrate a VM or LXC container to another node")]
    public static async Task<string> MigrateVm([Description("Cluster name")] string cluster_name,
                                               [Description("VM ID")] int vmid,
                                               [Description("Target node name")] string target_node,
                                               [Description("Live migration — keep VM running during migration (optional, default false)")] bool online,
                                               [Description("Target storage for disks (optional)")] string? target_storage,
                                               IAiServerService aiServerService,
                                               IPermissionService permissionService,
                                               CommandExecutor commandExecutor)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.MigrateVm))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        var vm = (await clusterClient.CachedData.GetResourcesAsync(false))
                      .FirstOrDefault(a => a.ResourceType == ClusterResourceType.Vm && a.VmId == vmid);

        if (vm == null) { return JsonSerializer.Serialize(new { error = $"VM {vmid} not found" }); }

        if (!await permissionService.HasVmAsync(cluster_name, ClusterPermissions.Vm.Migrate, vmid))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var result = await commandExecutor.ExecuteAsync(new VmMigrateCommand(cluster_name, vmid, target_node, online, target_storage));

        return result.IsSuccess
            ? JsonSerializer.Serialize(new { success = true, vmid, target_node, online, upid = result.Upid })
            : JsonSerializer.Serialize(new { error = result.ErrorMessage });
    }

    [McpServerTool, Description("Create a backup of a VM or LXC container via vzdump")]
    public static async Task<string> BackupVm([Description("Cluster name")] string cluster_name,
                                              [Description("VM ID")] int vmid,
                                              [Description("Target storage ID (optional)")] string? storage,
                                              [Description("Backup mode: snapshot, suspend, stop (optional)")] string? mode,
                                              [Description("Compression: 0, 1, gzip, lzo, zstd (optional)")] string? compress,
                                              [Description("Mark backup as protected (optional)")] bool? protected_backup,
                                              [Description("Limit I/O bandwidth in KiB/s (optional)")] int? bwlimit,
                                              IAiServerService aiServerService,
                                              IPermissionService permissionService,
                                              CommandExecutor commandExecutor)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.BackupVm))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        var vm = (await clusterClient.CachedData.GetResourcesAsync(false))
                      .FirstOrDefault(a => a.ResourceType == ClusterResourceType.Vm && a.VmId == vmid);

        if (vm == null) { return JsonSerializer.Serialize(new { error = $"VM {vmid} not found" }); }

        if (!await permissionService.HasVmAsync(cluster_name, ClusterPermissions.Vm.Backup, vmid))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var result = await commandExecutor.ExecuteAsync(new VmBackupCommand(cluster_name, vmid, storage, mode, compress, protected_backup, null, bwlimit));

        return result.IsSuccess
            ? JsonSerializer.Serialize(new { success = true, vmid, storage, mode, upid = result.Upid })
            : JsonSerializer.Serialize(new { error = result.ErrorMessage });
    }

    [McpServerTool, Description("Get historical metrics data (CPU, Memory, Network, Disk I/O) for VMs")]
    public static async Task<string> ListVmRrdData([Description("Cluster name")] string cluster_name,
                                                   [Description("Array of VM IDs to get RRD data for")] int[] vmids,
                                                   [Description("Time frame: Hour, Day (default), Week, Month, Year (optional)")] RrdDataTimeFrame time_frame,
                                                   [Description("Consolidation method: Average (default), Maximum (optional)")] RrdDataConsolidation consolidation,
                                                   [Description("Metric categories: Cpu, Memory, Disk, Network, Pressure (PSI, PVE 9.0+), All (default) (optional)")] RrdMetricCategory metrics,
                                                   IAiServerService aiServerService)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.ListVmRrdData))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        var allVms = (await clusterClient.CachedData.GetResourcesAsync(false))
                           .Where(a => a.ResourceType == ClusterResourceType.Vm)
                           .ToList();

        allVms = [.. await aiServerService.HasAsync(cluster_name, allVms)];

        var results = new List<object>();

        foreach (var vmid in vmids)
        {
            var vm = allVms.FirstOrDefault(a => a.VmId == vmid);

            if (vm == null)
            {
                results.Add(new { vmid, error = $"VM with ID {vmid} not found" });
                continue;
            }

            var rrdData = await clusterClient.CachedData.GetRrdDataAsync(vm.Node, vm.VmType, vm.VmId, time_frame, consolidation, false);

            results.Add(new
            {
                vmid = vm.VmId,
                name = vm.Name,
                node = vm.Node,
                type = vm.Type,
                data = rrdData.Select(d =>
                {
                    var obj = new Dictionary<string, object> { ["time"] = d.TimeDate };

                    if (metrics.HasFlag(RrdMetricCategory.Cpu))
                    {
                        obj["cpu_usage"] = d.CpuUsagePercentage;
                        obj["cpu_total"] = d.CpuSize;
                    }

                    if (metrics.HasFlag(RrdMetricCategory.Memory))
                    {
                        obj["memory_usage"] = d.MemoryUsage;
                        obj["memory_total"] = d.MemorySize;
                    }

                    if (metrics.HasFlag(RrdMetricCategory.Disk))
                    {
                        obj["disk_usage"] = d.DiskUsage;
                        obj["disk_total"] = d.DiskSize;
                        obj["disk_read"] = d.DiskRead;
                        obj["disk_write"] = d.DiskWrite;
                    }

                    if (metrics.HasFlag(RrdMetricCategory.Network))
                    {
                        obj["network_in"] = d.NetIn;
                        obj["network_out"] = d.NetOut;
                    }

                    if (metrics.HasFlag(RrdMetricCategory.Pressure))
                    {
                        obj["pressure_cpu_some"] = d.PressureCpuSome;
                        obj["pressure_cpu_full"] = d.PressureCpuFull;
                        obj["pressure_io_some"] = d.PressureIoSome;
                        obj["pressure_io_full"] = d.PressureIoFull;
                        obj["pressure_memory_some"] = d.PressureMemorySome;
                        obj["pressure_memory_full"] = d.PressureMemoryFull;
                    }

                    return obj;
                }).ToArray()
            });
        }

        return JsonSerializer.Serialize(new { vms = results });
    }
}
