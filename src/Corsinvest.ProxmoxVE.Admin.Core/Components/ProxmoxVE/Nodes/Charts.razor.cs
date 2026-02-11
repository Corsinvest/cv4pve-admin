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

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
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
            _refreshLock?.Release();
        }
    }

    public void Dispose()
    {
        _refreshLock?.Dispose();
        GC.SuppressFinalize(this);
    }
}
