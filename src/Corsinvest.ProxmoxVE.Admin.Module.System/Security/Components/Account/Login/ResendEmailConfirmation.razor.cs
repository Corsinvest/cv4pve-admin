/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Module.System.Security.Services;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Components.Account.Login;

public partial class ResendEmailConfirmation(UserManager<ApplicationUser> userManager,
                                             IEmailSender<ApplicationUser> emailSender,
                                             NavigationManager navigationManager,
                                             RequestLimiter requestLimiter,
                                             ICurrentUserService currentUserService,
                                             IAuditService auditService)
{
    // Same limits as the password reset: enough for a lost message, too few to flood a mailbox
    private const int MaxPerEmail = 3;
    private const int MaxPerAddress = 10;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);

    private InputModel Input { get; set; } = new();
    private bool IsBusy { get; set; }
    private bool Sent { get; set; }

    private sealed class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    private async Task OnValidSubmitAsync(InputModel model)
    {
        IsBusy = true;
        try
        {
            // The answer is the same in every case: it must not tell whether the account exists
            var address = currentUserService.IpAddress;
            if (!requestLimiter.TryAcquire($"resend-confirmation:address:{address}", MaxPerAddress, Window)
                || !requestLimiter.TryAcquire($"resend-confirmation:email:{model.Email.Trim().ToUpperInvariant()}", MaxPerEmail, Window))
            {
                await auditService.LogAsync("ResendEmailConfirmation", false, $"Too many requests, no message sent. Email: {model.Email}");
                return;
            }

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user is null || await userManager.IsEmailConfirmedAsync(user)) { return; }

            await userManager.SendConfirmationAsync(user, navigationManager, emailSender);
            await auditService.LogAsync("ResendEmailConfirmation", true, $"Confirmation link sent. Email: {model.Email}");
        }
        finally
        {
            IsBusy = false;
            Sent = true;
        }
    }
}
