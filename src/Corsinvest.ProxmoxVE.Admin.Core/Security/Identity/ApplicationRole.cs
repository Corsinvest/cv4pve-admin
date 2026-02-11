using Microsoft.AspNetCore.Identity;

namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

public class ApplicationRole : IdentityRole, IDescription, IBuiltIn
{
    public bool BuiltIn { get; set; }
    public bool Default { get; set; }
    public string Description { get; set; } = default!;
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = [];
    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = [];
    public virtual ICollection<ApplicationRoleClaim> Claims { get; set; } = [];
}
