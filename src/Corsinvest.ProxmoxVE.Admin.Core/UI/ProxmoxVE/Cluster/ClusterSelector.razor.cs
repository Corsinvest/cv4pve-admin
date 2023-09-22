/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Service;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Cluster;

public partial class ClusterSelector
{
    [Parameter] public EventCallback<string> ClusterNameChanged { get; set; }
    [Parameter] public string ClusterName { get; set; } = default!;
    [Parameter] public bool OpenPve { get; set; }
    [Parameter] public RenderFragment<ClusterOptions> RenderRow { get; set; } = default!;

    [Inject] private IPveClientService PveClientService { get; set; } = default!;
    [Inject] private IBrowserService BrowserService { get; set; } = default!;

    private async Task ValueChanged(string value)
    {
        ClusterName = await PveClientService.ClusterIsValidAsync(value)
                                ? value
                                : string.Empty;
        await ClusterNameChanged.InvokeAsync(ClusterName);
        StateHasChanged();
    }

    public async Task OpenUrl(ClusterOptions item) => await BrowserService.Open(PveClientService.GetUrl(item), "_blank");
}