using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security;

public static class Permissions
{
    public static string BaseName { get; } = "Security";

    public static class Users
    {
        public static PermissionsCrud Data { get; } = new(BaseName, nameof(Users), nameof(Data));
        public static Permission ResetPassword { get; } = new(Data.Prefix, nameof(ResetPassword), "Reset Password");
        public static PermissionsCrud Permissions { get; } = new(Data.Prefix, nameof(Permissions));
    }

    public static class Roles
    {
        public static PermissionsCrud Data { get; } = new(BaseName, nameof(Roles), nameof(Data));
        public static PermissionsCrud Permissions { get; } = new(Data.Prefix, nameof(Permissions), "Permissions");
    }
}
