/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Vm;

public partial class Charts(IAdminService adminService) : IRefreshableData, IClusterName, IDisposable
{
    [EditorRequired, Parameter] public IClusterResourceVm Vm { get; set; } = default!;
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;

    private RrdDataTimeFrame RrdDataTimeFrame { get; set; } = RrdDataTimeFrame.Day;
    private RrdDataConsolidation RrdDataConsolidation { get; set; } = RrdDataConsolidation.Average;
    private IEnumerable<VmRrdData> Items { get; set; } = [];

    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private bool _disposed;

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        if (_disposed || !await _refreshLock.WaitAsync(0)) { return; }

        try
        {
            Items = await adminService[ClusterName].CachedData.GetRrdDataAsync(Vm.Node,
                                                                               Vm.VmType,
                                                                               Vm.VmId,
                                                                               RrdDataTimeFrame,
                                                                               RrdDataConsolidation,
                                                                               false);
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
