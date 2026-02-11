using MimeKit;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Services;

internal sealed class EmailSenderService(ISettingsService settingsService,
                                         IEmailSender emailSender) : IEmailSender<ApplicationUser>
{
    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        => await SendEmailAsync(email,
                                "Confirm your email",
                                $"""
                                <html>
                                <body>
                                    <h2>Email Confirmation</h2>
                                    <p>Hello {user.DisplayName ?? user.UserName},</p>
                                    <p>Thank you for registering. Please confirm your email address by clicking the button below:</p>
                                    <p><a href='{confirmationLink}' style='display:inline-block;padding:10px 20px;background-color:#007bff;color:white;text-decoration:none;border-radius:5px;'>Confirm Email</a></p>
                                    <p>If the button doesn't work, copy and paste this link into your browser:</p>
                                    <p><a href='{confirmationLink}'>{confirmationLink}</a></p>
                                    <p>If you did not request this, please ignore this email.</p>
                                </body>
                                </html>
                                """);

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        => await SendEmailAsync(email,
                                "Reset your password",
                                $"""
                                <html>
                                <body>
                                    <h2>Password Reset</h2>
                                    <p>Hello {user.DisplayName ?? user.UserName},</p>
                                    <p>You requested to reset your password. Click the button below to reset it:</p>
                                    <p><a href='{resetLink}' style='display:inline-block;padding:10px 20px;background-color:#dc3545;color:white;text-decoration:none;border-radius:5px;'>Reset Password</a></p>
                                    <p>If the button doesn't work, copy and paste this link into your browser:</p>
                                    <p><a href='{resetLink}'>{resetLink}</a></p>
                                    <p>If you did not request this, please ignore this email and your password will remain unchanged.</p>
                                </body>
                                </html>
                                """);

    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        => await SendEmailAsync(email,
                                "Reset your password",
                                $"""
                                <html>
                                <body>
                                    <h2>Password Reset Code</h2>
                                    <p>Hello {user.DisplayName ?? user.UserName},</p>
                                    <p>You requested to reset your password. Use the following code to reset it:</p>
                                    <p style='font-size:24px;font-weight:bold;color:#007bff;letter-spacing:2px;'>{resetCode}</p>
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
