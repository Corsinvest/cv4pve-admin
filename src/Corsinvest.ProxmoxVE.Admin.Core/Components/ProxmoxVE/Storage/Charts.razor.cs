/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Storage;

public partial class Charts(IAdminService adminService) : IRefreshableData, INode, IClusterName
{
    [EditorRequired, Parameter] public string Node { get; set; } = default!;
    [EditorRequired, Parameter] public string Storage { get; set; } = default!;
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;

    private RrdDataTimeFrame RrdDataTimeFrame { get; set; } = RrdDataTimeFrame.Day;
    private RrdDataConsolidation RrdDataConsolidation { get; set; } = RrdDataConsolidation.Average;
    private IEnumerable<NodeStorageRrdData> Items { get; set; } = [];

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();
    public async Task RefreshDataAsync()
    {
        Items = await adminService[ClusterName].CachedData.GetRrdDataAsync(Node, Storage, RrdDataTimeFrame, RrdDataConsolidation, false);

        if (!Items.Any()) { return; }
    }
}
