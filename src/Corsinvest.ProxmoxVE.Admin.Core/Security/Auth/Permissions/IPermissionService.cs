using Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

public interface IPermissionService
{
    Task<bool> HasAsync(string clusterName, Permission permission);
    Task<bool> HasAsync(string clusterName, Permission permission, string path);
    Task<bool> HasAsync(string clusterName, string permissionKey, string path);
    Task<bool> HasAsync(string userId, string clusterName, string permissionKey, string path);
    Task<bool> HasAnyAsync(string userId, string clusterName, IEnumerable<string> permissionKeys, string path);
    Task<bool> HasAllAsync(string userId, string clusterName, IEnumerable<string> permissionKeys, string path);

    Task AddForUserAsync(string userId, string clusterName, string permissionKey, string path, bool propagated, bool builtIn);
    Task AddForUserAsync(string userId, IEnumerable<(string clusterName, string permissionKey, string path, bool propagated, bool builtIn)> permissions);
    Task RemoveForUserAsync(string userId, string clusterName, string permissionKey, string path);
    Task RemoveForUserAsync(string userId, IEnumerable<(string clusterName, string permissionKey, string path)> permissions);

    Task AddForRoleAsync(string roleId, string clusterName, string permissionKey, string path, bool propagated, bool builtIn);
    Task AddForRoleAsync(string roleId, IEnumerable<(string clusterName, string permissionKey, string path, bool propagated, bool builtIn)> permissions);
    Task RemoveForRoleAsync(string roleId, string clusterName, string permissionKey, string path);
    Task RemoveForRoleAsync(string roleId, IEnumerable<(string clusterName, string permissionKey, string path)> permissions);

    Task<List<UserPermission>> GetUserPermissionsAsync(string userId);
    Task<List<RolePermission>> GetRolePermissionsAsync(string roleId);

    ValueTask ClearCacheAsync();
}
