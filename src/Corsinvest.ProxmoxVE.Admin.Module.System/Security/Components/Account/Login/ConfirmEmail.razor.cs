/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Components.Account.Login;

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
        var code = DecodeCode();
        if (user is null || code is null)
        {
            _message = L["Invalid confirmation link."];
            _alertStyle = AlertStyle.Danger;
            return;
        }

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

    private string? DecodeCode()
    {
        try
        {
            return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code!));
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
