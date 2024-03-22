/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.UI.Layout;

public partial class PveWidgets
{
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    private bool Valid { get; set; }
    private string MailTo { get; set; } = "#";

    protected override async Task OnInitializedAsync()
    {
        MailTo = "#";
        try
        {
            Valid = await PveClientService.ExistsCurrentClusterNameAsync();

            var data = await PveAdminHelper.GetSupportInfo(await PveClientService.GetClientCurrentClusterAsync());
            MailTo = $"mailto:support@corsinvets.it?subject=Request%20quote%20license&body=" + 
                        data.ToArray().Select(a => Uri.HexEscape(a)).JoinAsString("");
        }
        catch { }
    }
}