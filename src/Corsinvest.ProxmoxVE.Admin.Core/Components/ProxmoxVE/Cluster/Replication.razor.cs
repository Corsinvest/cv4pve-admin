/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Cluster;

public partial class Replication(IAdminService adminService) : IRefreshableData, IClusterName, IDisposable
{
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;
    [Parameter] public string Style { get; set; } = default!;

    private IEnumerable<ClusterReplication> Items { get; set; } = default!;
    private bool IsLoading { get; set; }
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        if (!await _refreshLock.WaitAsync(0)) { return; }
        try
        {
            IsLoading = true;
            var client = await adminService[ClusterName].GetPveClientAsync();
            Items = await client.Cluster.Replication.GetAsync();
        }
        finally
        {
            IsLoading = false;
            _refreshLock?.Release();
        }
    }

    public void Dispose()
    {
        _refreshLock?.Dispose();
        GC.SuppressFinalize(this);
    }
}
