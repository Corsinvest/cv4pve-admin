/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.UI.Layout;

public partial class PveWidgets
{
    [Inject] private IPveClientService PveClientService { get; set; } = default!;
    private bool Valid { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Valid = await PveClientService.IsValidCurrentClusterName();
        }
        catch { }
    }
}