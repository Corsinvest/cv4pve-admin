namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public class Credential
{
    public string Username { get; set; } = default!;

    [Encrypt]
    public string Password { get; set; } = default!;
}
