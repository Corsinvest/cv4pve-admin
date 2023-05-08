/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Vm;

public partial class ToolBarStatus
{
    [Parameter] public Size Size { get; set; } = Size.Medium;
    [Parameter] public bool CanNoVnc { get; set; }
    [Parameter] public bool CanChangeStatus { get; set; }
    [Parameter] public IClusterResourceVm Vm { get; set; } = default!;
    [Parameter] public EventCallback<VmStatus> OnStatusChanged { get; set; }
    [Parameter] public RenderFragment ToolBarContent { get; set; } = default!;
    [Parameter] public EventCallback OnShowConsole { get; set; }
    private async Task ChageStatus(VmStatus status) => await OnStatusChanged.InvokeAsync(status);
}
