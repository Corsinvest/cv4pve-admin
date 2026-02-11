using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Vm;

public partial class Charts(IAdminService adminService) : IRefreshableData, IClusterName
{
    [EditorRequired, Parameter] public IClusterResourceVm Vm { get; set; } = default!;
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;

    private RrdDataTimeFrame RrdDataTimeFrame { get; set; } = RrdDataTimeFrame.Day;
    private RrdDataConsolidation RrdDataConsolidation { get; set; } = RrdDataConsolidation.Average;
    private IEnumerable<VmRrdData> Items { get; set; } = [];

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        Items = await adminService[ClusterName].CachedData.GetRrdDataAsync(Vm.Node,
                                                                           Vm.VmType,
                                                                           Vm.VmId,
                                                                           RrdDataTimeFrame,
                                                                           RrdDataConsolidation,
                                                                           false);
        await InvokeAsync(StateHasChanged);
    }
}
