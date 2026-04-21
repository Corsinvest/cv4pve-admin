/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */

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

        await userManager.SendPasswordResetAsync(user!, navigationManager, EmailSender);

        IsBusy = false;
        navigationManager.NavigateTo("/ForgotPasswordConfirmation");
    }
}
