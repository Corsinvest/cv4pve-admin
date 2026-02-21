/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Nodes;

public partial class Summary(IAdminService adminService) : IRefreshableData, IClusterName, IDisposable
{
    [EditorRequired, Parameter] public IClusterResourceNode Node { get; set; } = default!;
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;

    private NodeStatus? Status { get; set; }

    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private bool _disposed;

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
        if (_disposed || !await _refreshLock.WaitAsync(0)) { return; }

        try
        {
            var client = await adminService[ClusterName].GetPveClientAsync();
            Status = await client.Nodes[Node.Node].Status.GetAsync();
            await InvokeAsync(StateHasChanged);
        }
        finally
        {
            if (!_disposed) { _refreshLock.Release(); }
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _refreshLock.Dispose();
    }
}
