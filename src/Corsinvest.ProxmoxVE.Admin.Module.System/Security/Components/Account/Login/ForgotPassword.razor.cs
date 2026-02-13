/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.WebUtilities;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Components.Account.Login;

public partial class ForgotPassword(UserManager<ApplicationUser> userManager,
                                    IEmailSender<ApplicationUser> EmailSender,
                                    NavigationManager navigationManager)
{
    private InputModel Input { get; set; } = new();
    private bool IsBusy { get; set; }

    private sealed class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    private async Task OnValidSubmitAsync(InputModel model)
    {
        IsBusy = true;
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is null || !await userManager.IsEmailConfirmedAsync(user))
        {
            // Don't reveal that the user does not exist or is not confirmed
            navigationManager.NavigateTo("/ForgotPasswordConfirmation");
            return;
        }

        var code = await userManager.GeneratePasswordResetTokenAsync(user!);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = navigationManager.GetUriWithQueryParameters(navigationManager.ToAbsoluteUri("/ResetPassword").AbsoluteUri,
                                                                      new Dictionary<string, object?> { ["code"] = code });

        await EmailSender.SendPasswordResetLinkAsync(user!, Input.Email, HtmlEncoder.Default.Encode(callbackUrl));

        IsBusy = false;
        navigationManager.NavigateTo("/ForgotPasswordConfirmation");
    }
}
