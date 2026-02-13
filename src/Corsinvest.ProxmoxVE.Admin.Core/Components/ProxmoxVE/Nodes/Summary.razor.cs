/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Nodes;

public partial class Summary(IAdminService adminService) : IRefreshableData, IClusterName
{
    [EditorRequired, Parameter] public IClusterResourceNode Node { get; set; } = default!;
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;

    private NodeStatus? Status { get; set; }

    private string CpuModel
    {
        get
        {
            if (Status?.CpuInfo == null) { return string.Empty; }

            var cpuInfo = Status.CpuInfo;
            var info = cpuInfo.Sockets > 1
                        ? "Sockets"
                        : "Socket";

            return $"{cpuInfo.Model} {cpuInfo.Sockets} ({info})";
        }
    }

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();
    public async Task RefreshDataAsync()
    {
        var client = await adminService[ClusterName].GetPveClientAsync();
        Status = await client.Nodes[Node.Node].Status.GetAsync();
        await InvokeAsync(StateHasChanged);
    }
}
