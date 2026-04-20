/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Encodings.Web;
using MimeKit;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Services;

internal sealed class EmailSenderService(ISettingsService settingsService,
                                         IEmailSender emailSender) : IEmailSender<ApplicationUser>
{
    private static string DisplayNameOf(ApplicationUser user)
        => HtmlEncoder.Default.Encode(user.DisplayName ?? user.UserName ?? string.Empty);

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        => SendEmailAsync(email,
                          "Confirm your email",
                                $"""
                                <html>
                                <body>
                                    <h2>Email Confirmation</h2>
                                    <p>Hello {DisplayNameOf(user)},</p>
                                    <p>Thank you for registering. Please confirm your email address by clicking the button below:</p>
                                    <p><a href='{HtmlEncoder.Default.Encode(confirmationLink)}' style='display:inline-block;padding:10px 20px;background-color:#007bff;color:white;text-decoration:none;border-radius:5px;'>Confirm Email</a></p>
                                    <p>If the button doesn't work, copy and paste this link into your browser:</p>
                                    <p><a href='{HtmlEncoder.Default.Encode(confirmationLink)}'>{HtmlEncoder.Default.Encode(confirmationLink)}</a></p>
                                    <p>If you did not request this, please ignore this email.</p>
                                </body>
                                </html>
                                """);

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        => SendEmailAsync(email,
                          "Reset your password",
                                $"""
                                <html>
                                <body>
                                    <h2>Password Reset</h2>
                                    <p>Hello {DisplayNameOf(user)},</p>
                                    <p>You requested to reset your password. Click the button below to reset it:</p>
                                    <p><a href='{HtmlEncoder.Default.Encode(resetLink)}' style='display:inline-block;padding:10px 20px;background-color:#dc3545;color:white;text-decoration:none;border-radius:5px;'>Reset Password</a></p>
                                    <p>If the button doesn't work, copy and paste this link into your browser:</p>
                                    <p><a href='{HtmlEncoder.Default.Encode(resetLink)}'>{HtmlEncoder.Default.Encode(resetLink)}</a></p>
                                    <p>If you did not request this, please ignore this email and your password will remain unchanged.</p>
                                </body>
                                </html>
                                """);

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        => SendEmailAsync(email,
                          "Reset your password",
                                $"""
                                <html>
                                <body>
                                    <h2>Password Reset Code</h2>
                                    <p>Hello {DisplayNameOf(user)},</p>
                                    <p>You requested to reset your password. Use the following code to reset it:</p>
                                    <p style='font-size:24px;font-weight:bold;color:#007bff;letter-spacing:2px;'>{HtmlEncoder.Default.Encode(resetCode)}</p>
                                    <p>This code will expire in 15 minutes.</p>
                                    <p>If you did not request this, please ignore this email and your password will remain unchanged.</p>
                                </body>
                                </html>
                                """);

    private async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new MimeMessage();
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };

        await emailSender.SendEmailAsync(message, settingsService.GetAppSettings().SmtpEmailConfig);
    }
}
