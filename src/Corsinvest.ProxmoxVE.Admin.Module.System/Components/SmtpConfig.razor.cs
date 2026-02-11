using MimeKit;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Components;

public partial class SmtpConfig(IEmailSender emailSender, NotificationService notificationService)
{
    [Parameter] public AppSettings Settings { get; set; } = default!;
    [Parameter] public EventCallback<AppSettings> SettingsChanged { get; set; } = default!;

    private string TestEmailAddress { get; set; } = string.Empty;
    private bool IsSendingTest { get; set; }

    private async Task SendTestEmailAsync()
    {
        if (string.IsNullOrWhiteSpace(TestEmailAddress))
        {
            notificationService.Warning(L["Please enter a test email address"]);
            return;
        }

        if (!TestEmailAddress.Contains('@'))
        {
            notificationService.Warning(L["Please enter a valid email address"]);
            return;
        }

        IsSendingTest = true;
        try
        {
            var message = new MimeMessage();
            message.To.Add(MailboxAddress.Parse(TestEmailAddress));
            message.Subject = L["Test Email from cv4pve-admin"];
            message.Body = new TextPart("plain")
            {
                Text = L["This is a test email to verify SMTP configuration.\n\nIf you receive this email, your SMTP settings are correct."]
            };

            await emailSender.SendEmailAsync(message, Settings.SmtpEmailConfig);

            notificationService.Success(L["Test email sent successfully to {0}", TestEmailAddress]);
        }
        catch (Exception ex)
        {
            notificationService.Error(L["Error"], L["Failed to send test email: {0}", ex.Message]);
        }
        finally
        {
            IsSendingTest = false;
        }
    }
}
