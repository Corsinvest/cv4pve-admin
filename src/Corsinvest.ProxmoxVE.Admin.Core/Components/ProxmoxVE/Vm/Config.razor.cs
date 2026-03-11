/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Vm;

public partial class Config(IAdminService adminService) : IRefreshableData, IClusterName
{
    [EditorRequired, Parameter] public IClusterResourceVm Vm { get; set; } = default!;
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;

    private VmConfig VmConfig { get; set; } = default!;

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        VmConfig = await adminService[ClusterName].CachedData.GetVmConfigAsync(Vm.Node, Vm.VmType, Vm.VmId, false);
        await InvokeAsync(StateHasChanged);
    }
}
