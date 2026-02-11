using Microsoft.AspNetCore.Identity;

namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

public class ApplicationUser : IdentityUser, IBuiltIn
{
    public string? DisplayName { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ProfileImageUrl { get; set; }
    public bool BuiltIn { get; set; }

    public static string GetUserProfileImagePath(string email) => Path.Combine(ApplicationHelper.UserProfileImagesPath, $"{email}.jpg");
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = [];

    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = [];
    public virtual ICollection<ApplicationUserClaim> Claims { get; set; } = [];
    public virtual ICollection<ApplicationUserLogin> Logins { get; set; } = [];
    public virtual ICollection<ApplicationUserToken> Tokens { get; set; } = [];
    //[Required]
    //public string DefaultCulture { get; set; } = "en-US";

    //public string? TimeZoneId { get; set; }
    //public string? LanguageCode { get; set; }
}
