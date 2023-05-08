/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.VmUnlock;

internal static class Helper
{
    public static async Task Unlock(PveClient client, IEnumerable<IClusterResourceVm> resources)
    {
        foreach (var item in resources)
        {
            switch (item.VmType)
            {
                case VmType.Lxc: await client.Nodes[item.Node].Lxc[item.VmId].Config.UpdateVm(delete: "lock"); break;
                case VmType.Qemu: await client.Nodes[item.Node].Qemu[item.VmId].Config.UpdateVm(delete: "lock", skiplock: true); break;
                default: break;
            }
        }
    }

    public static async Task<IEnumerable<ClusterResource>> GetVmLocks(PveClient client)
    {
        //check prev version
        //var count = 0;
        //foreach (var nodeItem in client.GetNodes().Where(a => a.IsOnline))
        //{
        //    var node = client.Nodes[nodeItem.Node];

        //    count += node.Qemu.Vmlist().ToEnumerable()
        //                      .Select(a => (IDictionary<string, object>)a)
        //                      .Count(a => a.ContainsKey("lock") && !string.IsNullOrWhiteSpace(a["lock"] + ""));

        //    count += node.Lxc.Vmlist().ToEnumerable()
        //                     .Select(a => (IDictionary<string, object>)a)
        //                     .Count(a => a.ContainsKey("lock") && !string.IsNullOrWhiteSpace(a["lock"] + ""));
        //}

        //return count;
        //CalculateHostUsage
        return (await client.GetResources(ClusterResourceType.All))
                    .CalculateHostUsage()
                    .Where(a => a.ResourceType == ClusterResourceType.Vm)
                    .Where(a => a.IsLocked);
    }
}
