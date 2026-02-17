/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Nodes;

public partial class Charts(IAdminService adminService) : IRefreshableData, INode, IClusterName, IDisposable
{
    [EditorRequired, Parameter] public string Node { get; set; } = default!;
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;

    private RrdDataTimeFrame RrdDataTimeFrame { get; set; } = RrdDataTimeFrame.Day;
    private RrdDataConsolidation RrdDataConsolidation { get; set; } = RrdDataConsolidation.Average;
    private IEnumerable<NodeRrdData> Items { get; set; } = [];
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private bool _disposed;

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        if (_disposed) { return; }
        if (!await _refreshLock.WaitAsync(0)) { return; }
        try
        {
            Items = await adminService[ClusterName].CachedData.GetRrdDataAsync(Node,
                                                                               RrdDataTimeFrame,
                                                                               RrdDataConsolidation,
                                                                               false);
        }
        finally
        {
            if (!_disposed) { _refreshLock?.Release(); }
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _refreshLock?.Dispose();
        GC.SuppressFinalize(this);
    }
}
