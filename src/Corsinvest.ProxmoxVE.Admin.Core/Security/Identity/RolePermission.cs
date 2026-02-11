namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

public class RolePermission : BasePermission
{
    public string RoleId { get; set; } = default!;
    [Required] public ApplicationRole Role { get; set; } = default!;
}
