/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;
using Corsinvest.ProxmoxVE.Admin.Module.System.Security.Services;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Components.Account.Login;

public partial class ForgotPassword(UserManager<ApplicationUser> userManager,
                                    IEmailSender<ApplicationUser> EmailSender,
                                    NavigationManager navigationManager,
                                    RequestLimiter requestLimiter,
                                    ICurrentUserService currentUserService,
                                    IAuditService auditService)
{
    // Enough for who mistypes or loses the first message, too few to flood a mailbox
    private const int MaxPerEmail = 3;
    private const int MaxPerAddress = 10;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);

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

        // Counted for every email, also one of no user: the answer must not tell whether the user exists
        var address = currentUserService.IpAddress;
        if (!requestLimiter.TryAcquire($"forgot-password:address:{address}", MaxPerAddress, Window)
            || !requestLimiter.TryAcquire($"forgot-password:email:{model.Email.Trim().ToUpperInvariant()}", MaxPerEmail, Window))
        {
            await auditService.LogAsync("ForgotPassword", false, $"Too many requests, no message sent. Email: {model.Email}");
            navigationManager.NavigateTo("/ForgotPasswordConfirmation");
            return;
        }

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
