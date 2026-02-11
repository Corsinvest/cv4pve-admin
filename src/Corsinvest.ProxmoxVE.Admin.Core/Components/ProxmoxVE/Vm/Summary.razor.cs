using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Vm;

public partial class Summary(IAdminService adminService) : IRefreshableData, IClusterName
{
    [EditorRequired, Parameter] public IClusterResourceVm Vm { get; set; } = default!;
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;

    private VmBaseStatusCurrent? Status { get; set; }

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        var client = await adminService[ClusterName].GetPveClientAsync();
        Status = await client.GetVmStatusAsync(Vm.Node, Vm.VmType, Vm.VmId);
        await InvokeAsync(StateHasChanged);
    }
}
