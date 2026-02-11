using Corsinvest.ProxmoxVE.Admin.Core.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Services;
using Corsinvest.ProxmoxVE.Admin.Core.Validators;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;
using OperationResult = FluentResults.Result;

namespace Corsinvest.ProxmoxVE.Admin.Module.Notifier.Smtp;

public class Settings : NotifierConfiguration, INotifierEmail
{
    [Required]
    public SmtpEmailConfig SmtpConfig { get; set; } = new();

    [Required, EmailAddresses]
    public string ToAddresses { get; set; } = default!;

    protected override async Task<OperationResult> SendImpAsync(NotifierMessage message, IServiceProvider serviceProvider)
    {
        try
        {
            var email = new MimeMessage { Subject = message.Subject };

            email.To.AddRange(ToAddresses.Split([',', ';', '+'], StringSplitOptions.RemoveEmptyEntries)
                                         .Select(a => a.Trim())
                                         .Select(MailboxAddress.Parse));

            var builder = new BodyBuilder { HtmlBody = message.Body };

            foreach (var item in message.Attachments)
            {
                if (item.Stream != null) { builder.Attachments.Add(item.Name, item.Stream); }
            }

            email.Body = builder.ToMessageBody();

            var emailSender = serviceProvider.GetRequiredService<IEmailSender>();
            await emailSender.SendEmailAsync(email, SmtpConfig);

            return OperationResult.Ok();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail($"SMTP error: {ex.Message}");
        }
    }
}
