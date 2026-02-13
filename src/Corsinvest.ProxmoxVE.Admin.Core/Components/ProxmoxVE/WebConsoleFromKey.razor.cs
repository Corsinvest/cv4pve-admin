/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE;

public partial class WebConsoleFromKey(IFusionCache fusionCache)
{
    [Parameter] public string Key { get; set; } = default!;

    private WebConsoleInfo? Info { get; set; }
    private bool Initalized { get; set; }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            Info = WebConsoleInfo.Decode(fusionCache, Key);
            Initalized = true;
            StateHasChanged();
        }
    }
}
