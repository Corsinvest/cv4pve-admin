/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Components;

public partial class Support(ISettingsService settingsService, IAdminService adminService)
{
    private string WhoUsing { get; set; } = string.Empty;
    private string Info { get; set; } = string.Empty;
    private IEnumerable<ClusterSettings> ClustersSettings { get; set; } = [];

    protected override void OnInitialized() => ClustersSettings = settingsService.GetEnabledClustersSettings();

    private async Task ValueChanged(string clusterName)
    {
        Info = string.Empty;
        WhoUsing = string.Empty;

        var client = await adminService[clusterName].GetPveClientAsync();
        if (client != null)
        {
            WhoUsing = await GenerateWhoUsingAsync(client);
            Info = await GetClusterInfoAsync(client, ClustersSettings.FromName(clusterName)!);
        }
    }

    private static async Task<string> GenerateWhoUsingAsync(PveClient client)
    {
        var items = (await client.GetResourcesAsync(ClusterResourceType.All)).CalculateHostUsage();
        var lxc = items.Count(a => a.ResourceType == ClusterResourceType.Vm && a.VmType == VmType.Lxc);
        var qemu = items.Count(a => a.ResourceType == ClusterResourceType.Vm && a.VmType == VmType.Qemu);
        var nodes = items.Where(a => a.ResourceType == ClusterResourceType.Node && a.IsOnline);
        var allStorage = items.Where(a => a.ResourceType == ClusterResourceType.Storage && a.IsAvailable);

        var storages = allStorage.Where(a => !a.Shared).ToList();
        storages.AddRange(allStorage.Where(a => a.Shared).DistinctBy(a => a.Storage));

        var version = new List<string>();
        foreach (var item in await client.Nodes.GetAsync())
        {
            version.Add((await client.Nodes[item.Node].Version.GetAsync()).Version);
        }

        return @$"Proxmox VE Version: {version.Distinct().Order().JoinAsString(" , ")}
Host Number: {nodes.Count()}
CPUs: {nodes.Sum(a => a.CpuSize)}
Memory: {FormatHelper.FromBytes(nodes.Sum(a => a.MemorySize))}
Storage: {FormatHelper.FromBytes(storages.Sum(a => a.DiskSize))}
VM/CT Number: {qemu}/{lxc}
Company:

        ";
    }

    private static async Task<string> GetClusterInfoAsync(PveClient client, ClusterSettings clusterSettings)
    {
        var rows = new List<IEnumerable<string>>();
        var status = await client.Cluster.Status.GetAsync();

        foreach (var item in status.Where(a => !string.IsNullOrWhiteSpace(a.IpAddress)).OrderBy(a => a.Name))
        {
            clusterSettings.GetNodeSettings(item.IpAddress, item.Name);

            var version = item.IsOnline
                            ? (await client.Nodes[item.Name].Version.GetAsync())?.Version
                            : string.Empty;

            rows.Add(
            [
                "??-ServerId-??",//nodeSettings?.ServerId!,
                version!,
                item.Name,
                item.IpAddress,
                "??--SubscriptionId--??" //nodeSettings?.SubscriptionId!
            ]);
        }

        return TableGenerator.ToText(["Server Id", "PVE Version", "Name", "Ip Address", "Subscription Id"], rows);
    }
}
