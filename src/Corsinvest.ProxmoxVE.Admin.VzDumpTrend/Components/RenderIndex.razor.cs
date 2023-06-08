/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Services;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;

namespace Corsinvest.ProxmoxVE.Admin.VzDumpTrend.Components;

public partial class RenderIndex
{
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    private PveClient PveClient { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            PveClient = await PveClientService.GetClientCurrentCluster();
        }
        catch { }
    }

    private async Task<IEnumerable<NodeStorageContent>> GetBackupsInline()
    {
        //StateHasChanged();
        var ret = new List<NodeStorageContent>();

        foreach (var node in (await PveClient.GetNodes()).Where(a => a.IsOnline))
        {
            ret.AddRange(await PveClient.Nodes[node.Node].GetBackupsInAllStorages());
        }

        return ret.Distinct().ToList();
    }

    private async Task<IEnumerable<ClusterResource>> GetNotScheduled()
    {
        var ret = new List<ClusterResource>();
        var backups = (await PveClient.Cluster.Backup.Get()).Where(a => a.Enabled);

        if (!backups.Any(a => a.All))
        {
            var vmIdsInBackup = backups.SelectMany(a => a.VmId.Split(",").Where(a => !string.IsNullOrWhiteSpace(a)))
                                       .Select(a => long.Parse(a))
                                       .ToList();

            var vms = (await PveClient.GetResources(ClusterResourceType.All))
                                .Where(a => a.ResourceType == ClusterResourceType.Vm && !a.IsTemplate);

            ret = vms.Where(a => !vmIdsInBackup.Contains(a.VmId)).ToList();
        }

        return ret;
    }
}

//TODO add gannt scheduling backups