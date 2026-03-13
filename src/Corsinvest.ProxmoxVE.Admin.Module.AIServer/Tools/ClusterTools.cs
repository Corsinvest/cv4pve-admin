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
internal static class ClusterTools
{
    [McpServerTool, Description("List all configured clusters with their name and description")]
    public static string ListClusters(IAdminService adminService, IAiServerService aiServerService)
    {
        var clusters = adminService.Where(a => a.Settings.Enabled)
                                   .OrderBy(c => c.Settings.Name)
                                   .Select(c => new
                                   {
                                       name = c.Settings.Name,
                                       description = c.Settings.Description
                                   });

        return aiServerService.SerializeTable(clusters);
    }

    [McpServerTool, Description("Get health summary of a cluster: node count (online/offline), VM count (running/stopped/paused), storage count")]
    public static async Task<string> GetClusterStatus([Description("Cluster name")] string cluster_name,
                                                      IAiServerService aiServerService)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.GetClusterStatus))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        var pveClient = await clusterClient.GetPveClientAsync();
        var status = (await pveClient.Cluster.Status.GetAsync()).ToArray();

        var clusterInfo = status.FirstOrDefault(a => a.Type == PveConstants.KeyApiCluster);

        var resources = await clusterClient.CachedData.GetResourcesAsync(false);
        var vms = resources.Where(a => a.ResourceType == ClusterResourceType.Vm && a.Status != PveConstants.StatusUnknown).ToList();
        vms = [.. await aiServerService.HasAsync(cluster_name, vms)];

        return JsonSerializer.Serialize(new
        {
            cluster = cluster_name,
            cluster_name = clusterInfo?.Name,
            quorate = clusterInfo?.Quorate == 1,
            nodes_total = clusterInfo?.Nodes,
            nodes_online = status.Count(a => a.Type != PveConstants.KeyApiCluster && a.IsOnline),
            vms = new
            {
                total = vms.Count,
                running = vms.Count(v => v.Status == PveConstants.StatusVmRunning),
                stopped = vms.Count(v => v.Status == PveConstants.StatusVmStopped),
                paused = vms.Count(v => v.Status == PveConstants.StatusVmPaused)
            }
        });
    }

    [McpServerTool, Description("Get cluster options: migration type, network, bandwidth limit, HA settings, console type")]
    public static async Task<string> GetClusterOptions([Description("Cluster name")] string cluster_name,
                                                       IAiServerService aiServerService)
    {
        if (!await aiServerService.CanExecuteToolAsync(cluster_name, Permissions.Tools.GetClusterOptions))
        {
            return JsonSerializer.Serialize(new { error = "Permission denied" });
        }

        var (clusterClient, errorJson) = aiServerService.GetClusterClient(cluster_name);
        if (clusterClient == null) { return errorJson!; }

        var pveClient = await clusterClient.GetPveClientAsync();
        var options = await pveClient.Cluster.Options.GetAsync();

        return JsonSerializer.Serialize(new
        {
            migration_type = options.Migration?.Type,
            migration_network = options.Migration?.Network,
            console = options.Console,
            keyboard = options.Keyboard,
            mac_prefix = options.MacPrefix,
            description = options.Description
        });
    }
}
