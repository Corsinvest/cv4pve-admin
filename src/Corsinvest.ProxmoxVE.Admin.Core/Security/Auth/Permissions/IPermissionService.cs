/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
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

    Task AddForUserAsync(string userId, IEnumerable<PermissionData> permissions);
    Task RemoveForUserAsync(string userId, IEnumerable<PermissionData> permissions);

    Task AddForRoleAsync(string roleId, IEnumerable<PermissionData> permissions);
    Task RemoveForRoleAsync(string roleId, IEnumerable<PermissionData> permissions);

    Task<List<UserPermission>> GetUserPermissionsAsync(string userId);
    Task<List<RolePermission>> GetRolePermissionsAsync(string roleId);
    Task SyncForUserAsync(string userId, IEnumerable<UserPermission> newPermissions);
    Task SyncForRoleAsync(string roleId, IEnumerable<RolePermission> newPermissions);

    // AppToken permissions
    Task<bool> HasAsync(Guid appTokenId, string clusterName, string permissionKey, string path);
    Task AddForAppTokenAsync(Guid appTokenId, IEnumerable<PermissionData> permissions);
    Task RemoveForAppTokenAsync(Guid appTokenId, IEnumerable<PermissionData> permissions);
    Task<List<AppTokenPermission>> GetAppTokenPermissionsAsync(Guid appTokenId);
    Task SyncForAppTokenAsync(Guid appTokenId, IEnumerable<AppTokenPermission> newPermissions);

    ValueTask ClearCacheAsync();

    Task<bool> PveHasAsync(string clusterName, string pvePermission, string path);
}
