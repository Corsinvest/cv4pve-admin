/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Storage;

namespace Corsinvest.ProxmoxVE.Admin.ClusterUsage.Components;

public partial class Storages
{
    [Inject] private IPveClientService PveClientService { get; set; } = default!;
    private PveClient PveClient { get; set; } = default!;

    protected override async Task OnInitializedAsync() => PveClient = await PveClientService.GetClientCurrentClusterAsync();
    private async Task<IEnumerable<StorageItem>> GetConfigStorages() => (await PveClient.Storage.Get()).OrderBy(a => a.Storage);
    private async Task<IEnumerable<ClusterResource>> GetStorages() => await PveClient.GetResources(ClusterResourceType.Storage);

    private async Task<IEnumerable<ClusterResource>> GetVms()
        => (await PveClient.GetResources(ClusterResourceType.All))
            .CalculateHostUsage()
            .Where(a => a.ResourceType == ClusterResourceType.Vm);

    private async Task<IEnumerable<NodeStorageContent>> GetContents(ClusterResource item)
    {
        var ret = new List<NodeStorageContent>();

        switch (item.ResourceType)
        {
            case ClusterResourceType.All: break;
            case ClusterResourceType.Node: break;
            case ClusterResourceType.Vm:
                foreach (var node in (await PveClient.GetNodes()).Where(a => a.IsOnline))
                {
                    foreach (var storage in (await PveClient.Nodes[node.Node].Storage.Get(enabled: true)).Where(a => a.Active && a.Enabled))
                    {
                        ret.AddRange(await PveClient.Nodes[node.Node].Storage[storage.Storage].Content.Get(vmid: Convert.ToInt32(item.VmId)));
                    }
                }
                break;

            case ClusterResourceType.Storage:
                ret.AddRange(await PveClient.Nodes[item.Node].Storage[item.Storage].Content.Get());
                break;

            case ClusterResourceType.Pool: break;
            case ClusterResourceType.Sdn: break;
            case ClusterResourceType.Unknown: break;
            default: break;
        }

        return ret.Distinct()
                  .OrderBy(a => a.Storage)
                  .ThenBy(a => a.Content)
                  .ThenBy(a => a.VmId)
                  .ThenBy(a => a.Volume);
    }
}