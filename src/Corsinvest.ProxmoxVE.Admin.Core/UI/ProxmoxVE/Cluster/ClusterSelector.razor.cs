/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.JSInterop;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Cluster;

public partial class ClusterSelector
{
    [Parameter] public EventCallback<string> ClusterNameChanged { get; set; }
    [Parameter] public string ClusterName { get; set; } = default!;
    [Parameter] public bool OpenPve { get; set; }

    [Inject] private IPveClientService PveClientService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private async Task ValueChanged(string value)
    {
        ClusterName = await PveClientService.ClusterIsValid(value)
                                ? value
                                : string.Empty;
        await ClusterNameChanged.InvokeAsync(ClusterName);
        StateHasChanged();
    }

    public async Task OpenUrl(ClusterOptions item) => await JSRuntime.InvokeVoidAsync("open", PveAdminHelper.GetPveUrl(item), "_blank");
}