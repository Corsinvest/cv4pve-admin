/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api;

namespace Corsinvest.ProxmoxVE.Admin.AutoSnap.Components;

public partial class RenderWidgetInfo
{
    [Inject] private IServiceScopeFactory ScopeFactory { get; set; } = default!;

    private string Description { get; set; } = default!;

    private async Task<bool> GetStatusAsync(PveClient client, string clusterName)
    {
        var ret = false;
        Description = "AutoSnap";

        var (scheduled, _, snapCount, _, inError) = await Helper.InfoAsync(ScopeFactory, clusterName);
        if (scheduled == 0) { Description = L["AutoSnap not scheduled!"]; }
        else if (inError > 0) { Description = L["AutoSnap in error!"]; }
        else if (snapCount == 0) { Description = L["AutoSnap not found!"]; }
        else { ret = true; }

        StateHasChanged();
        return ret;
    }
}
