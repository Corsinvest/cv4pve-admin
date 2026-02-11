using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;

public record Role(string Key,
                   string Description,
                   bool Default,
                   bool BuiltIn,
                   IEnumerable<Permission> Permissions)
{
    public Role(IEnumerable<Permission> Permissions) : this(string.Empty, string.Empty, false, true, Permissions) { }
}
