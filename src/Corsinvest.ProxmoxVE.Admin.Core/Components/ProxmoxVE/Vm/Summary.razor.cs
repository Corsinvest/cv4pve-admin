/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Vm;

public partial class Summary(IAdminService adminService) : IRefreshableData, IClusterName, IDisposable
{
    private record HistoryPoint(int X, double Cpu, double Ram);

    private readonly Queue<HistoryPoint> History = new();
    private int _historyIndex;
    private const int MaxHistory = 20;

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
            if (Status != null)
            {
                if (History.Count >= MaxHistory) { History.Dequeue(); }
                History.Enqueue(new HistoryPoint(_historyIndex++,
                                                      Math.Round(Status.CpuUsagePercentage * 100, 1),
                                                      Math.Round(Status.MemoryUsagePercentage * 100, 1)));
            }
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
