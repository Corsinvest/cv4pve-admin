/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Common;

public partial class Tasks(DialogService dialogService,
                           IAdminService adminService) : IRefreshableData, INode, IVmId, IClusterName
{
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;
    [Parameter] public string Node { get; set; } = default!;
    [Parameter] public long VmId { get; set; }
    [Parameter] public string Style { get; set; } = default!;

    private bool IsLoading { get; set; }
    private bool ForNode => !string.IsNullOrEmpty(Node);
    private bool ForVm => VmId > 0;
    private IEnumerable<NodeTask> Items { get; set; } = default!;
    private IList<NodeTask> SelectedItems { get; set; } = [];
    private bool _validColumnClick;

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        IsLoading = true;
        SelectedItems.Clear();

        var client = await adminService[ClusterName].GetPveClientAsync();

        Items = ForNode
                    ? await client.Nodes[Node]
                                  .Tasks.GetAsync(limit: 500,
                                                  start: 0,
                                                  vmid: ForVm
                                                        ? Convert.ToInt32(VmId)
                                                        : null)
                    : await client.Cluster.Tasks.GetAsync();

        IsLoading = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task RowSelectAsync(NodeTask item)
    {
        if (_validColumnClick) { await ShowEditorAsync(item); }
    }

    private static void RowRender(RowRenderEventArgs<NodeTask> args)
    {
        if (!args.Data!.StatusOk) { args.SetRowStyleError(); }
    }

    private void CellClick(DataGridCellMouseEventArgs<NodeTask> e) => _validColumnClick = nameof(NodeTask.Status) == e.Column!.Property;

    private async Task ShowEditorAsync(NodeTask item)
    {
        var client = await adminService[ClusterName].GetPveClientAsync();

        await dialogService.OpenSideLogAsync(L["Task viewer: {0}", item.Description],
                                                     (await client.Nodes[item.Node]
                                                                  .Tasks[item.UniqueTaskId]
                                                                  .Log.GetAsync(limit: 10000))
                                                     .JoinAsString(Environment.NewLine));
    }
}
