/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json;
using Corsinvest.ProxmoxVE.Admin.Module.AIServer.Services;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Module.AIServer.Tools;

[McpServerToolType]
internal static class StorageTools
{
    [McpServerTool, Description("List cluster storage with usage statistics and type")]
    public static async Task<string> ListStorage([Description("Cluster name")] string cluster_name,
                                                 [Description("Storage type filter: dir, lvm, lvmthin, zfs, cephfs, etc. (optional)")] string? type,
                                                 [Description("Detail level: Minimal (default) or Full (optional)")] VmTools.DetailLevel detail_level,
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

        return detail_level == VmTools.DetailLevel.Full
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

    [McpServerTool, Description("List backups available on a node storage")]
    public static async Task<string> ListBackups([Description("Cluster name")] string cluster_name,
                                                 [Description("Node name")] string node,
                                                 [Description("Storage name")] string storage,
                                                 [Description("VM ID filter (optional)")] int? vmid,
                                                 IAiServerService aiServerService)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.ListBackups))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        var storageResource = (await clusterClient.CachedData.GetResourcesAsync(false))
                               .FirstOrDefault(a => a.ResourceType == ClusterResourceType.Storage
                                                    && a.Status != PveConstants.StatusUnknown
                                                    && a.Node.Equals(node, StringComparison.OrdinalIgnoreCase)
                                                    && a.Storage.Equals(storage, StringComparison.OrdinalIgnoreCase));

        if (storageResource == null)
        {
            return JsonSerializer.Serialize(new { error = $"Storage '{storage}' not found on node '{node}'" });
        }

        if (!(await aiServerService.HasAsync(cluster_name, [storageResource])).Any())
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var allowedVmIds = await GetAllowedVmIdsAsync(cluster_name, clusterClient, aiServerService);

        var client = await clusterClient.GetPveClientAsync();
        var contents = await client.Nodes[node].Storage[storage].Content.GetAsync("backup", vmid);

        var results = contents.Where(c => c.VmId == 0 || allowedVmIds.Contains(c.VmId))
                              .OrderByDescending(c => c.CreationDate)
                              .Select(c => new
                              {
                                  storage = c.Storage,
                                  volid = c.Volume,
                                  vmid = c.VmId,
                                  format = c.Format,
                                  size = c.Size,
                                  size_info = c.SizeInfo,
                                  creation = c.CreationDate,
                                  notes = c.Notes,
                                  encrypted = c.Encrypted,
                                  verified = c.Verified
                              });

        return aiServerService.SerializeTable(results);
    }

    [McpServerTool, Description("List storage content (ISO, templates, images, backups)")]
    public static async Task<string> ListStorageContent([Description("Cluster name")] string cluster_name,
                                                        [Description("Storage name")] string storage,
                                                        [Description("Node name")] string node,
                                                        [Description("Content type filter: iso, vztmpl, backup, images (optional)")] string? content_type,
                                                        IAiServerService aiServerService)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.ListStorageContent))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        var allStorages = (await clusterClient.CachedData.GetResourcesAsync(false))
                           .Where(a => a.ResourceType == ClusterResourceType.Storage
                                       && a.Status != PveConstants.StatusUnknown
                                       && a.Storage.Equals(storage, StringComparison.OrdinalIgnoreCase)
                                       && a.Node.Equals(node, StringComparison.OrdinalIgnoreCase))
                           .ToList();

        allStorages = [.. await aiServerService.HasAsync(cluster_name, allStorages)];

        var targetStorage = allStorages.FirstOrDefault();

        if (targetStorage == null)
        {
            return JsonSerializer.Serialize(new { error = $"Storage '{storage}' not found on node '{node}'" });
        }

        var allowedVmIds = await GetAllowedVmIdsAsync(cluster_name, clusterClient, aiServerService);

        var client = await clusterClient.GetPveClientAsync();

        try
        {
            var contents = await client.Nodes[targetStorage.Node].Storage[storage].Content.GetAsync(content_type);
            var results = contents.Where(c => c.VmId == 0 || allowedVmIds.Contains(c.VmId))
                                  .OrderBy(c => c.Content).ThenBy(c => c.Volume)
                                  .Select(c => (object)new
                                  {
                                      volid = c.Volume,
                                      content = c.Content,
                                      content_description = c.ContentDescription,
                                      format = c.Format,
                                      size = c.Size,
                                      size_info = c.SizeInfo,
                                      vmid = c.VmId,
                                      creation = c.CreationDate,
                                      name = c.Name
                                  });

            return aiServerService.SerializeTable(results);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool, Description("List backup jobs and their schedule configuration")]
    public static async Task<string> ListBackupJobs([Description("Cluster name")] string cluster_name,
                                                    IAiServerService aiServerService)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.ListBackupJobs))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        var client = await clusterClient.GetPveClientAsync();
        var results = (await client.Cluster.Backup.GetAsync()).Select(a => new
        {
            id = a.Id,
            node = a.Node,
            schedule = a.Schedule,
            enabled = a.Enabled,
            vmids = (a.VmId + string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(v => int.TryParse(v.Trim(), out var id) ? (int?)id : null)
                         .Where(id => id.HasValue).Select(id => id!.Value).ToList(),
            storage = a.Storage,
            compress = a.Compress,
            mode = a.Mode
        }).ToList();

        return JsonSerializer.Serialize(new { backup_jobs = results });
    }

    private static async Task<HashSet<long>> GetAllowedVmIdsAsync(string cluster_name,
                                                                  ClusterClient clusterClient,
                                                                  IAiServerService aiServerService)
    {
        var allVms = (await clusterClient.CachedData.GetResourcesAsync(false))
                         .Where(a => a.ResourceType == ClusterResourceType.Vm).ToList();
        return [.. (await aiServerService.HasAsync(cluster_name, allVms)).Select(v => v.VmId)];
    }
}
