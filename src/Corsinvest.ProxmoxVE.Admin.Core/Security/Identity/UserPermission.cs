namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

public class UserPermission : BasePermission
{
    public string UserId { get; set; } = default!;
    [Required] public ApplicationUser User { get; set; } = default!;
}
