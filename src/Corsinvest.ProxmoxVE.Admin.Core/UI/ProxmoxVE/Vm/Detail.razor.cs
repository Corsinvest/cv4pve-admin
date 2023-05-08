/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Vm;

public partial class Detail
{
    [Parameter] public Func<Task<VmBaseStatusCurrent>> GetStatus { get; set; } = default!;
    [Parameter] public Func<Task<VmQemuAgentGetFsInfo>> GetVmQemuAgentGetFsInfo { get; set; } = default!;

    private VmBaseStatusCurrent? VmStatus { get; set; }
    private VmQemuAgentGetFsInfo? FsInfo { get; set; }
    private VmType VmType { get; set; }

    protected override async Task OnInitializedAsync() => await Refresh();

    public async Task Refresh()
    {
        VmStatus = await GetStatus();
        VmType = VmStatus switch
        {
            VmLxcStatusCurrent _ => VmType.Lxc,
            VmQemuStatusCurrent _ => VmType.Qemu,
            _ => throw new ArgumentOutOfRangeException(),
        };

        if (VmType == VmType.Qemu) { FsInfo = await GetVmQemuAgentGetFsInfo(); }
    }

    private static Color ValueToColor(double value)
        => value switch
        {
            double x when x >= 80 => Color.Error,
            double x when x >= 70 => Color.Warning,
            _ => Color.Primary,
        };
}
