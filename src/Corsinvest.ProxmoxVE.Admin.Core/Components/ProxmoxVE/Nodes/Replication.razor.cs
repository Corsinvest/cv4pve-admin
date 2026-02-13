/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;
using Mapster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Nodes;

public partial class Replication(IAdminService adminService,
                                 DialogService dialogService) : IRefreshableData, INode, IClusterName
{
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;
    [Parameter] public string Node { get; set; } = default!;
    [Parameter] public string Style { get; set; } = default!;
    [Parameter] public long? VmId { get; set; } = default!;
    [Parameter] public bool CanScheduleNow { get; set; }

    private bool IsLoading { get; set; }
    private IEnumerable<Data> Items { get; set; } = default!;
    private IList<Data> SelectedItems { get; set; } = [];
    private Data SelectedItem => SelectedItems[0];

    private class Data : NodeReplication, INode
    {
        public string Node { get; set; } = default!;
    }

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        IsLoading = true;

        var clusterClient = adminService[ClusterName];
        async Task<List<Data>> GetData(string node)
        {
            var data = (await clusterClient.CachedData.GetReplicationsAsync(node, VmId, false))
                           .AsQueryable()
                           .ProjectToType<Data>()
                           .ToList();

            data.ForEach(a => a.Node = node);

            return data;
        }

        if (string.IsNullOrEmpty(Node))
        {
            var items = new List<Data>();
            foreach (var item in (await clusterClient.CachedData.GetResourcesAsync(false)).Where(a => a.ResourceType == ClusterResourceType.Node && a.IsOnline))
            {
                items.AddRange(await GetData(item.Node));
            }

            Items = items;
        }
        else
        {
            Items = await GetData(Node);
        }

        IsLoading = false;
    }

    private static void RowRender(RowRenderEventArgs<Data> args)
    {
        if (!string.IsNullOrEmpty(args.Data!.Error)) { args.SetRowStyleError(); }
    }

    private async Task LogAsync()
    {
        var client = await adminService[ClusterName].GetPveClientAsync();
        await dialogService.OpenSideLogAsync(L["Log"],
                                             (await client.Nodes[SelectedItem.Node].Replication[SelectedItem.Id].Log.GetAsync())
                                                .JoinAsString(Environment.NewLine));
    }

    private async Task ScheduleNowAsync()
    {
        var client = await adminService[ClusterName].GetPveClientAsync();
        await client.Nodes[SelectedItem.Node]
                    .Replication[SelectedItem.Id]
                    .ScheduleNow
                    .ScheduleNow();
    }
}
