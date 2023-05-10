/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class PveAdminExtensions
{
    public static async Task<(string Type, string Name)> GetClusterInfo(this PveClient client)
    {
        var status = await client.Cluster.Status.Get();
        var clusterName = status.FirstOrDefault(a => a.Type == PveConstants.KeyApiCluster)?.Name;
        var type = string.IsNullOrEmpty(clusterName)
                        ? "NODE"
                        : "CLUSTER";

        var name = string.IsNullOrEmpty(clusterName)
                        ? status.FirstOrDefault()!.Name
                        : clusterName;
        return (type, name);
    }

    public static async Task<IEnumerable<VmRrdData>> GetVmRrdData(this PveClient pveClient,
                                                                  IClusterResourceVm vm,
                                                                  RrdDataTimeFrame rrdDataTimeFrame,
                                                                  RrdDataConsolidation rrdDataConsolidation)
           => vm.VmType switch
           {
               VmType.Qemu => await pveClient.Nodes[vm.Node].Qemu[vm.VmId].Rrddata.Get(rrdDataTimeFrame, rrdDataConsolidation),
               VmType.Lxc => await pveClient.Nodes[vm.Node].Lxc[vm.VmId].Rrddata.Get(rrdDataTimeFrame, rrdDataConsolidation),
               _ => throw new IndexOutOfRangeException(),
           };

    public static async Task<VmBaseStatusCurrent> GetVmStatus(this PveClient pveClient, IClusterResourceVm vm)
        => vm.VmType switch
        {
            VmType.Qemu => await pveClient.Nodes[vm.Node].Qemu[vm.VmId].Status.Current.Get(),
            VmType.Lxc => await pveClient.Nodes[vm.Node].Lxc[vm.VmId].Status.Current.Get(),
            _ => throw new ArgumentOutOfRangeException(vm.VmType.ToString()),
        };

    public static async Task<IEnumerable<string>> GetVmsJollyKeys(this PveClient client,
                                                                  bool addAll,
                                                                  bool addNodes,
                                                                  bool addPools,
                                                                  bool addTags,
                                                                  bool addVmId,
                                                                  bool addVmName)
    {
        var vmIds = new List<string>();
        var resources = await client.GetResources(ClusterResourceType.All);

        if (addAll) { vmIds.Add("@all"); }

        if (addNodes)
        {
            vmIds.AddRange(resources.Where(a => a.ResourceType == ClusterResourceType.Node && a.IsOnline)
                                    .OrderBy(a => a.Node)
                                    .Select(a => $"@all-{a.Node}"));

            vmIds.AddRange(resources.Where(a => a.ResourceType == ClusterResourceType.Node && a.IsOnline)
                                    .OrderBy(a => a.Node)
                                    .Select(a => $"@node-{a.Node}"));
        }

        if (addPools)
        {
            vmIds.AddRange(resources.Where(a => a.ResourceType == ClusterResourceType.Pool)
                                    .OrderBy(a => a.Pool)
                                    .Select(a => $"@pool-{a.Pool}"));
        }

        if (addTags)
        {
            var tags = (await client.Cluster.Options.Get()).AllowedTags ?? new List<string>();
            vmIds.AddRange(tags.Select(a => $"@tag-{a}"));
        }

        var vms = resources.Where(a => a.ResourceType == ClusterResourceType.Vm && !a.IsUnknown);
        if (addVmId) { vmIds.AddRange(vms.Select(a => a.VmId + "").OrderBy(a => a)); }
        if (addVmName) { vmIds.AddRange(vms.Select(a => a.Name).OrderBy(a => a)); }

        return vmIds.Distinct();
    }
}
