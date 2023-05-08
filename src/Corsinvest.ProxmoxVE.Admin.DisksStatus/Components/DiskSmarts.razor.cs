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

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Smarts"];
        DataGridManager.DefaultSort = new() { [nameof(NodeDiskSmart.NodeDiskSmartAttribute.Name)] = false };
        DataGridManager.QueryAsync = async () => (await PveClient.Nodes[Node].Disks.Smart.Get(Disk)).Attributes;
    }
}