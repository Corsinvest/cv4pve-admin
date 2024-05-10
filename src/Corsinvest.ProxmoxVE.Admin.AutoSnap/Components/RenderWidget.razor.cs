/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.AutoSnap.Components;

public partial class RenderWidget
{
    [Inject] private IServiceScopeFactory ScopeFactory { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    private int Scheduled { get; set; }
    private DateTime? Last { get; set; }
    private int SnapCount { get; set; }
    private int VmScheduled { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        (Scheduled, Last, SnapCount, VmScheduled, _) = await Helper.InfoAsync(ScopeFactory, await PveClientService.GetCurrentClusterNameAsync());
        StateHasChanged();
    }
}