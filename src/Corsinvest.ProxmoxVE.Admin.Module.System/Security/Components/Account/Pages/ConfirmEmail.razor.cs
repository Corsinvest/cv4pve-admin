/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Components.Account.Pages;

public partial class ConfirmEmail(UserManager<ApplicationUser> userManager,
                                  NavigationManager navigationManager)
{
    [SupplyParameterFromQuery] private string? UserId { get; set; }
    [SupplyParameterFromQuery] private string? Code { get; set; }

    private string? _message;
    private AlertStyle _alertStyle;

    protected override async Task OnInitializedAsync()
    {
        if (UserId is null || Code is null)
        {
            navigationManager.NavigateTo("/");
            return;
        }

        var user = await userManager.FindByIdAsync(UserId);
        if (user is null)
        {
            _message = L["Invalid confirmation link."];
            _alertStyle = AlertStyle.Danger;
            return;
        }

        var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code));
        var result = await userManager.ConfirmEmailAsync(user, code);

        if (result.Succeeded)
        {
            _message = L["Email confirmed. You can now reset your password to login."];
            _alertStyle = AlertStyle.Success;
        }
        else
        {
            _message = L["Error confirming your email."];
            _alertStyle = AlertStyle.Danger;
        }
    }
}
