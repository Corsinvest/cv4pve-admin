using MailKit.Net.Smtp;
using MimeKit;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public sealed class SmtpEmailSender(ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendEmailAsync(MimeMessage message, SmtpEmailConfig config)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(config);

        if (message.To.Count == 0) { throw new ArgumentException("No recipient specified.", nameof(message)); }

        message.From.Add(new MailboxAddress(config.FromDisplayName, config.FromAddress));

        using var client = new SmtpClient();

        try
        {
            logger.LogDebug("Connecting to SMTP server {Host}:{Port}", config.Host, config.Port);
            await client.ConnectAsync(config.Host, config.Port, config.SecureSocketOption);

            if (!string.IsNullOrWhiteSpace(config.Username))
            {
                logger.LogDebug("Authenticating with username {Username}", config.Username);
                await client.AuthenticateAsync(config.Username, config.Password);
            }

            await client.SendAsync(message);

            logger.LogDebug("Disconnecting from SMTP server");
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Recipients} via {Host}:{Port}",
                           string.Join(", ", message.To), config.Host, config.Port);
            throw;
        }
    }
}
