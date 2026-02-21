/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Vm;

public partial class Summary(IAdminService adminService) : IRefreshableData, IClusterName, IDisposable
{
    [EditorRequired, Parameter] public IClusterResourceVm Vm { get; set; } = default!;
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;

    private VmBaseStatusCurrent? Status { get; set; }

    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private bool _disposed;

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        if (_disposed || !await _refreshLock.WaitAsync(0)) { return; }

        try
        {
            var client = await adminService[ClusterName].GetPveClientAsync();
            Status = await client.GetVmStatusAsync(Vm.Node, Vm.VmType, Vm.VmId);
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
