/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Nodes;

public partial class Disks(IAdminService adminService) : IRefreshableData, INode, IClusterName, IDisposable
{
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;
    [Parameter] public string Node { get; set; } = default!;
    [Parameter] public string Style { get; set; } = default!;
    [Parameter] public bool ShowSmartAttribute { get; set; } = true;

    private IEnumerable<NodeDiskList> Items { get; set; } = default!;
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
            Items = await client.Nodes[Node].Disks.List.GetAsync();
        }
        finally
        {
            IsLoading = false;
            _refreshLock?.Release();
        }
    }

    private static void RowRender(RowRenderEventArgs<NodeDiskList> args)
    {
        if (args.Data!.IsSsd && args.Data.Wearout != "N/A")
        {
            switch (PveAdminUIHelper.GetColorRange(100.0 - Convert.ToDouble(args.Data.Wearout)))
            {
                case Colors.Danger: args.SetRowStyleError(); break;
                case Colors.Warning: args.SetRowStyleWarning(); break;
                case Colors.Success: break;
                default: break;
            }
        }
    }

    public void Dispose()
    {
        _refreshLock?.Dispose();
        GC.SuppressFinalize(this);
    }
}
