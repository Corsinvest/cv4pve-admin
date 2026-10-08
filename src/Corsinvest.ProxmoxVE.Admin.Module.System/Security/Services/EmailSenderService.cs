/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Services;

internal sealed class EmailSenderService(ISettingsService settingsService,
                                         IEmailSender emailSender,
                                         IOptions<DataProtectionTokenProviderOptions> tokenOptions) : IAccountEmailSender
{
    private const string ColorBrand = "#1a4269";
    private const string ColorLink = "#2a6fae";

    private static string Encode(string? value) => HtmlEncoder.Default.Encode(value ?? string.Empty);

    private string Lifespan
    {
        get
        {
            var value = tokenOptions.Value.TokenLifespan;
            return value.TotalHours >= 48
                    ? $"{(int)value.TotalDays} days"
                    : value.TotalMinutes >= 120
                        ? $"{(int)value.TotalHours} hours"
                        : $"{(int)value.TotalMinutes} minutes";
        }
    }

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        => SendEmailAsync(email,
                          "Confirm your email",
                          Layout("Confirm your email",
                                 user,
                                 $"""
                                 <p style="margin:0 0 16px;">An account has been created for you. Click the button below to confirm your email address.</p>
                                 {Button(confirmationLink, "Confirm email")}
                                 """,
                                 $"This link expires in {Lifespan}. If you did not expect this email, you can safely ignore it.",
                                 confirmationLink));

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        => SendEmailAsync(email,
                          "Reset your password",
                          Layout("Reset your password",
                                 user,
                                 $"""
                                 <p style="margin:0 0 16px;">A password reset was requested for your account. Click the button below to set a new password.</p>
                                 {Button(resetLink, "Reset password")}
                                 """,
                                 $"This link expires in {Lifespan} and can be used only once. If you did not request it, you can safely ignore this email: your password stays unchanged.",
                                 resetLink));

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        => SendEmailAsync(email,
                          "Reset your password",
                          Layout("Reset your password",
                                 user,
                                 $"""
                                 <p style="margin:0 0 16px;">A password reset was requested for your account. Use the code below to set a new password.</p>
                                 <p style="margin:24px 0; padding:14px 16px; background:#f4f7fa; border-radius:6px; font-family:Consolas,monospace; font-size:14px; word-break:break-all;">{Encode(resetCode)}</p>
                                 """,
                                 $"This code expires in {Lifespan} and can be used only once. If you did not request it, you can safely ignore this email: your password stays unchanged."));

    public Task SendTwoFactorDisabledAsync(ApplicationUser user, bool byAdministrator)
        => SendEmailAsync(user.Email!,
                          "Two-factor authentication disabled",
                          Layout("Two-factor authentication disabled",
                                 user,
                                 $"""
                                 <p style="margin:0 0 16px;">{(byAdministrator
                                                                ? "An administrator turned off two-factor authentication for your account."
                                                                : "Two-factor authentication has been turned off for your account.")}</p>
                                 <p style="margin:0 0 24px;">You now sign in with your password only. To protect the account again, set up two-factor authentication from your profile.</p>
                                 """,
                                 "If you did not ask for this change, contact your administrator and change your password."));

    private static string Button(string link, string text)
        => $"""
           <table role="presentation" cellpadding="0" cellspacing="0" border="0" style="margin:24px 0;">
               <tr>
                   <td style="background:{ColorBrand}; border-radius:6px;">
                       <a href="{Encode(link)}" style="display:inline-block; padding:14px 32px; color:#ffffff; text-decoration:none; font-weight:600; font-size:15px;">{Encode(text)}</a>
                   </td>
               </tr>
           </table>
           <p style="margin:0 0 8px; font-size:13px; color:#555555;">Or copy and paste this link into your browser:</p>
           <p style="margin:0 0 24px; font-size:12px; word-break:break-all;"><a href="{Encode(link)}" style="color:{ColorLink};">{Encode(link)}</a></p>
           """;

    // Nested tables and inline styles: Outlook ignores a style block and has no flexbox
    private string Layout(string title, ApplicationUser user, string body, string note, string? link = null)
    {
        var appName = Encode(settingsService.GetAppSettings().AppName);
        var site = Uri.TryCreate(link, UriKind.Absolute, out var uri)
                    ? uri.GetLeftPart(UriPartial.Authority)
                    : null;

        return $"""
                <html>
                <body style="margin:0; padding:0;">
                <table role="presentation" cellpadding="0" cellspacing="0" border="0" style="width:100%; background:#f4f7fa; font-family:'Segoe UI',Arial,sans-serif; color:#111d24; padding:32px 0;">
                    <tr>
                        <td align="center">
                            <table role="presentation" cellpadding="0" cellspacing="0" border="0" style="width:600px; max-width:600px; background:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 8px 24px rgba(26,66,105,0.12);">
                                <tr>
                                    <td align="left" style="background:{ColorBrand}; padding:24px 32px; color:#ffffff; font-size:22px; font-weight:600;">{appName}</td>
                                </tr>
                                <tr>
                                    <td style="padding:32px; font-size:15px; line-height:1.6; color:#111d24;">
                                        <p style="margin:0 0 16px; font-size:18px; font-weight:600;">{Encode(title)}</p>
                                        <p style="margin:0 0 16px;">Hello {Encode(user.DisplayName ?? user.UserName)},</p>
                                        {body}
                                        <p style="margin:0; font-size:12px; color:#888888; line-height:1.5;">{Encode(note)}</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="background:#f4f7fa; padding:20px 32px; border-top:1px solid #dfe5eb; font-size:12px; color:#4a545f; line-height:1.5;">
                                        Sent by {appName}{(site == null ? string.Empty : $""" &middot; <a href="{Encode(site)}" style="color:{ColorLink}; text-decoration:none;">{Encode(site)}</a>""")}
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
                </body>
                </html>
                """;
    }

    private async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new MimeMessage();
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };

        await emailSender.SendEmailAsync(message, settingsService.GetAppSettings().SmtpEmailConfig);
    }
}
