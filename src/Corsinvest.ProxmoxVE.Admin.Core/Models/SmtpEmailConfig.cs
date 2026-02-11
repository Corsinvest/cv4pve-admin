using MailKit.Security;

namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public sealed class SmtpEmailConfig
{
    [Required]
    public string Host { get; set; } = default!;

    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    public SecureSocketOptions SecureSocketOption { get; set; }
        = SecureSocketOptions.StartTls;

    public string Username { get; set; } = string.Empty;

    [Encrypt]
    public string Password { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string FromAddress { get; set; } = default!;

    public string FromDisplayName { get; set; } = string.Empty;
}
