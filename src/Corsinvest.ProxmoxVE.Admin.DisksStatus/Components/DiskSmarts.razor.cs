/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;

namespace Corsinvest.ProxmoxVE.Admin.DisksStatus.Components;

public partial class DiskSmarts
{
    [Parameter] public string Node { get; set; } = default!;
    [Parameter] public string Disk { get; set; } = default!;
    [Parameter] public PveClient PveClient { get; set; } = default!;

    [Inject] private IDataGridManager<NodeDiskSmart.NodeDiskSmartAttribute> DataGridManager { get; set; } = default!;
    private NodeDiskSmart Data { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var data = await PveClient.Nodes[Node].Disks.Smart.GetAsync(Disk);
        data.Text = (data.Text + "").Replace("\n", "<br>");
        Data = data;

        if (Data.Attributes != null)
        {
            DataGridManager.Title = L["Smarts"];
            DataGridManager.DefaultSort = new() { [nameof(NodeDiskSmart.NodeDiskSmartAttribute.Name)] = false };
            DataGridManager.QueryAsync = async () => await Task.FromResult(Data.Attributes);
        }
    }
}
