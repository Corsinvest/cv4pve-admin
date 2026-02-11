using Microsoft.AspNetCore.Identity;

namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

public class ApplicationRoleClaim : IdentityRoleClaim<string>
{
    public virtual ApplicationRole Role { get; set; } = default!;
}
