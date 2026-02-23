/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
using ZiggyCreatures.Caching.Fusion;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Services;

public class PermissionService(ICurrentUserService currentUserService,
                               IFusionCache fusionCache,
                               IDbContextFactory<ModuleDbContext> dbContextFactory) : IPermissionService
{
    private const string CacheTag = "Permissions";

    public async Task<bool> HasAsync(string clusterName, Permission permission)
        => await HasAsync(clusterName, permission, "*");

    public async Task<bool> HasAsync(string clusterName, Permission permission, string path)
        => permission == null || await HasAsync(clusterName, permission.Key, path);

    public Task<bool> HasAsync(string clusterName, string permissionKey, string path)
    {
        var appTokenClaim = currentUserService.ClaimsPrincipal?.FindFirst(ApplicationClaimTypes.AppTokenId)?.Value;

        return appTokenClaim != null && Guid.TryParse(appTokenClaim, out var appTokenId)
            ? HasAsync(appTokenId, clusterName, permissionKey, path)
            : HasAsync(currentUserService.UserId, clusterName, permissionKey, path);
    }

    public async Task<bool> HasAsync(string userId, string clusterName, string permissionKey, string path)
    {
        if (string.IsNullOrEmpty(userId)) { return false; }

        var userPermissionsTask = GetUserPermissionsAsync(userId);
        var userRolesTask = GetUserRolesAsync(userId);
        await Task.WhenAll(userPermissionsTask, userRolesTask);

        return await HasWithRolesAsync(await userPermissionsTask, await userRolesTask, clusterName, permissionKey, path);
    }

    private async Task<Dictionary<string, List<string>>> GetRolesPermissionsAsync()
        => await fusionCache.GetOrSetAsync("Permissions:RolesClaims",
                                           async token =>
                                           {
                                               await using var db = await dbContextFactory.CreateDbContextAsync(token);

                                               return await db.RoleClaims.Where(a => a.ClaimType == ApplicationClaimTypes.Permission)
                                                                         .AsNoTracking()
                                                                         .Select(a => new { a.RoleId, a.ClaimValue })
                                                                         .GroupBy(a => a.RoleId)
                                                                         .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.ClaimValue!)
                                                                         .Distinct()
                                                                         .ToList(),
                                                       token);
                                           },
                                           TimeSpan.FromMinutes(10),
                                           tags: [CacheTag]);

    public async Task<List<UserPermission>> GetUserPermissionsAsync(string userId)
        => await fusionCache.GetOrSetAsync($"Permissions:UserId:{userId}",
                                           async token =>
                                           {
                                               await using var db = await dbContextFactory.CreateDbContextAsync(token);

                                               return await db.UserPermissions.AsNoTracking()
                                                                              .Where(a => a.UserId == userId)
                                                                              .ToListAsync(token);
                                           },
                                           TimeSpan.FromMinutes(10),
                                           tags: [CacheTag]);

    private async Task<List<string>> GetUserRolesAsync(string userId)
        => await fusionCache.GetOrSetAsync($"Permissions:UserRoles:{userId}",
                                           async token =>
                                           {
                                               await using var db = await dbContextFactory.CreateDbContextAsync(token);

                                               return await db.UserRoles.AsNoTracking()
                                                                        .Where(a => a.UserId == userId)
                                                                        .Select(a => a.RoleId)
                                                                        .ToListAsync(token);
                                           },
                                           TimeSpan.FromMinutes(10),
                                           tags: [CacheTag]);

    public async Task<List<RolePermission>> GetRolePermissionsAsync(string roleId)
        => await fusionCache.GetOrSetAsync($"Permissions:RolePermissions:{roleId}",
                                           async token =>
                                           {
                                               await using var db = await dbContextFactory.CreateDbContextAsync(token);

                                               return await db.RolePermissions.AsNoTracking()
                                                                              .Where(a => a.RoleId == roleId)
                                                                              .ToListAsync(token);
                                           },
                                           TimeSpan.FromMinutes(10),
                                           tags: [CacheTag]);

    public async Task<bool> HasAnyAsync(string userId,
                                        string clusterName,
                                        IEnumerable<string> permissionKeys,
                                        string path)
    {
        foreach (var key in permissionKeys)
        {
            if (await HasAsync(userId, clusterName, key, path)) { return true; }
        }
        return false;
    }

    public async Task<bool> HasAllAsync(string userId, string clusterName, IEnumerable<string> permissionKeys, string path)
    {
        foreach (var key in permissionKeys)
        {
            if (!await HasAsync(userId, clusterName, key, path)) { return false; }
        }
        return true;
    }

    public async Task RemoveForUserAsync(string userId, IEnumerable<PermissionData> permissions)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        await DeletePermissionsAsync(db.UserPermissions.Where(a => a.UserId == userId), permissions);
        await InvalidateUserPermissionsAsync(userId);
    }

    public async Task RemoveForRoleAsync(string roleId, IEnumerable<PermissionData> permissions)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        await DeletePermissionsAsync(db.RolePermissions.Where(a => a.RoleId == roleId), permissions);
        await InvalidateRolePermissionsAsync(roleId);
        await InvalidateUsersWithRoleAsync(roleId);
    }

    public async Task AddForRoleAsync(string roleId, IEnumerable<PermissionData> permissions)
    {
        var permissionsList = permissions.ToList();
        if (permissionsList.Count == 0) { return; }

        await using var db = await dbContextFactory.CreateDbContextAsync();

        if (!await db.Roles.AnyAsync(a => a.Id == roleId))
        {
            throw new InvalidOperationException($"Role with ID {roleId} does not exist");
        }

        var existingPermissions = await db.RolePermissions
                                          .Where(a => a.RoleId == roleId)
                                          .Select(a => new { a.ClusterName, a.PermissionKey, a.Path })
                                          .ToListAsync();

        var existingSet = existingPermissions.Select(a => $"{a.ClusterName}|{a.PermissionKey}|{a.Path}").ToHashSet();
        var newPermissions = permissionsList.Where(a => !existingSet.Contains($"{a.ClusterName}|{a.PermissionKey}|{a.Path}"))
                                            .Select(a => new RolePermission
                                            {
                                                RoleId = roleId,
                                                ClusterName = a.ClusterName,
                                                PermissionKey = a.PermissionKey,
                                                Path = a.Path,
                                                Propagated = a.Propagated,
                                                BuiltIn = a.BuiltIn
                                            })
                                            .ToList();

        if (newPermissions.Count != 0)
        {
            db.RolePermissions.AddRange(newPermissions);
            await db.SaveChangesAsync();
            await InvalidateRolePermissionsAsync(roleId);
            await InvalidateUsersWithRoleAsync(roleId);
        }
    }

    public async Task AddForUserAsync(string userId, IEnumerable<PermissionData> permissions)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("UserId cannot be null or empty", nameof(userId));
        }

        var permissionsList = permissions.ToList();
        if (permissionsList.Count == 0) { return; }

        await using var db = await dbContextFactory.CreateDbContextAsync();

        if (!await db.Users.AnyAsync(a => a.Id == userId))
        {
            throw new InvalidOperationException($"User with ID {userId} does not exist");
        }

        var existingPermissions = await db.UserPermissions
                                          .Where(a => a.UserId == userId)
                                          .Select(a => new { a.ClusterName, a.PermissionKey, a.Path })
                                          .ToListAsync();

        var existingSet = existingPermissions.Select(a => $"{a.ClusterName}|{a.PermissionKey}|{a.Path}").ToHashSet();
        var newPermissions = permissionsList.Where(p => !existingSet.Contains($"{p.ClusterName}|{p.PermissionKey}|{p.Path}"))
                                            .Select(a => new UserPermission
                                            {
                                                UserId = userId,
                                                ClusterName = a.ClusterName,
                                                PermissionKey = a.PermissionKey,
                                                Path = a.Path,
                                                Propagated = a.Propagated,
                                                BuiltIn = a.BuiltIn
                                            })
                                            .ToList();

        if (newPermissions.Count != 0)
        {
            db.UserPermissions.AddRange(newPermissions);
            await db.SaveChangesAsync();
            await InvalidateUserPermissionsAsync(userId);
        }
    }

    private async Task InvalidateUserPermissionsAsync(string userId)
    {
        await fusionCache.RemoveAsync($"Permissions:UserId:{userId}");
        await fusionCache.RemoveAsync($"Permissions:UserRoles:{userId}");
    }

    private async Task InvalidateUsersWithRoleAsync(string roleId)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();

        var usersWithRole = await db.UserRoles
                                    .AsNoTracking()
                                    .Where(ur => ur.RoleId == roleId)
                                    .Select(ur => ur.UserId)
                                    .ToListAsync();

        await Task.WhenAll(usersWithRole.SelectMany(userId => new[]
        {
            fusionCache.RemoveAsync($"Permissions:UserId:{userId}").AsTask(),
            fusionCache.RemoveAsync($"Permissions:UserRoles:{userId}").AsTask()
        }));
    }

    private async Task InvalidateRolePermissionsAsync(string roleId)
    {
        await fusionCache.RemoveAsync($"Permissions:RolePermissions:{roleId}");
        await fusionCache.RemoveAsync("Permissions:RolesClaims");
    }

    public async Task<bool> HasAsync(Guid appTokenId, string clusterName, string permissionKey, string path)
    {
        var tokenPermissionsTask = GetAppTokenPermissionsAsync(appTokenId);
        var tokenRolesTask = GetAppTokenRolesAsync(appTokenId);
        await Task.WhenAll(tokenPermissionsTask, tokenRolesTask);

        return await HasWithRolesAsync(await tokenPermissionsTask, await tokenRolesTask, clusterName, permissionKey, path);
    }

    private async Task<bool> HasWithRolesAsync(IEnumerable<BasePermission> directPermissions,
                                               List<string> roles,
                                               string clusterName,
                                               string permissionKey,
                                               string path)
    {
        if (directPermissions.Any(a => a.HasPermission(permissionKey, path, clusterName))) { return true; }

        if (roles.Count != 0)
        {
            var rolePermissionTasks = roles.Select(GetRolePermissionsAsync);
            var rolesClaimsTask = GetRolesPermissionsAsync();

            var allRolePermissions = await Task.WhenAll(rolePermissionTasks);
            var rolesClaims = await rolesClaimsTask;

            if (allRolePermissions.Any(rp => rp.Any(a => a.HasPermission(permissionKey, path, clusterName)))) { return true; }

            if (roles.Any(r => rolesClaims.TryGetValue(r, out var claims) && claims.Contains(permissionKey))) { return true; }
        }

        return false;
    }

    public async Task<List<AppTokenPermission>> GetAppTokenPermissionsAsync(Guid appTokenId)
        => await fusionCache.GetOrSetAsync($"Permissions:AppToken:{appTokenId}",
                                           async token =>
                                           {
                                               await using var db = await dbContextFactory.CreateDbContextAsync(token);
                                               return await db.AppTokenPermissions.AsNoTracking()
                                                                                  .Where(a => a.AppTokenId == appTokenId)
                                                                                  .ToListAsync(token);
                                           },
                                           TimeSpan.FromMinutes(10),
                                           tags: [CacheTag]);

    private async Task<List<string>> GetAppTokenRolesAsync(Guid appTokenId)
        => await fusionCache.GetOrSetAsync($"Permissions:AppTokenRoles:{appTokenId}",
                                           async token =>
                                           {
                                               await using var db = await dbContextFactory.CreateDbContextAsync(token);
                                               return await db.AppTokenRoles.AsNoTracking()
                                                                            .Where(a => a.AppTokenId == appTokenId)
                                                                            .Select(a => a.RoleId)
                                                                            .ToListAsync(token);
                                           },
                                           TimeSpan.FromMinutes(10),
                                           tags: [CacheTag]);

    public async Task AddForAppTokenAsync(Guid appTokenId, IEnumerable<PermissionData> permissions)
    {
        var list = permissions.ToList();
        if (list.Count == 0) { return; }

        await using var db = await dbContextFactory.CreateDbContextAsync();

        if (!await db.AppTokens.AnyAsync(a => a.Id == appTokenId))
        {
            throw new InvalidOperationException($"AppToken with ID {appTokenId} does not exist");
        }

        var existing = await db.AppTokenPermissions
                               .Where(a => a.AppTokenId == appTokenId)
                               .Select(a => new { a.ClusterName, a.PermissionKey, a.Path })
                               .ToListAsync();

        var existingSet = existing.Select(a => $"{a.ClusterName}|{a.PermissionKey}|{a.Path}").ToHashSet();
        var toAdd = list.Where(a => !existingSet.Contains($"{a.ClusterName}|{a.PermissionKey}|{a.Path}"))
                        .Select(a => new AppTokenPermission
                        {
                            AppTokenId = appTokenId,
                            ClusterName = a.ClusterName,
                            PermissionKey = a.PermissionKey,
                            Path = a.Path,
                            Propagated = a.Propagated,
                            BuiltIn = a.BuiltIn
                        })
                        .ToList();

        if (toAdd.Count != 0)
        {
            db.AppTokenPermissions.AddRange(toAdd);
            await db.SaveChangesAsync();
            await InvalidateAppTokenPermissionsAsync(appTokenId);
        }
    }

    public async Task RemoveForAppTokenAsync(Guid appTokenId, IEnumerable<PermissionData> permissions)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        await DeletePermissionsAsync(db.AppTokenPermissions.Where(a => a.AppTokenId == appTokenId), permissions);
        await InvalidateAppTokenPermissionsAsync(appTokenId);
    }

    private async Task InvalidateAppTokenPermissionsAsync(Guid appTokenId)
    {
        await fusionCache.RemoveAsync($"Permissions:AppToken:{appTokenId}");
        await fusionCache.RemoveAsync($"Permissions:AppTokenRoles:{appTokenId}");
    }

    private static async Task DeletePermissionsAsync<T>(IQueryable<T> baseQuery, IEnumerable<PermissionData> permissions)
        where T : BasePermission
    {
        foreach (var item in permissions)
        {
            var query = baseQuery;
            query = query.Where(a => a.ClusterName == item.ClusterName, !string.IsNullOrEmpty(item.ClusterName));
            query = query.Where(a => a.PermissionKey == item.PermissionKey, !string.IsNullOrEmpty(item.PermissionKey));
            query = query.Where(a => a.Path == item.Path, !string.IsNullOrEmpty(item.Path));
            await query.ExecuteDeleteAsync(CancellationToken.None);
        }
    }

    private static (List<PermissionData> ToRemove, List<PermissionData> ToAdd)
        CalcPermissionsDiff<T>(IEnumerable<T> current, IEnumerable<T> newItems) where T : BasePermission
    {
        var currentNonBuiltIn = current.Where(a => !a.BuiltIn).ToList();
        var newNonBuiltIn = newItems.Where(a => !a.BuiltIn).ToList();

        var toRemove = currentNonBuiltIn
            .Where(c => !newNonBuiltIn.Any(a => a.ClusterName == c.ClusterName
                                                && a.PermissionKey == c.PermissionKey
                                                && a.Path == c.Path))
            .Select(a => new PermissionData(a.ClusterName, a.PermissionKey, a.Path))
            .ToList();

        var toAdd = newNonBuiltIn
            .Where(n => !currentNonBuiltIn.Any(a => a.ClusterName == n.ClusterName
                                                    && a.PermissionKey == n.PermissionKey
                                                    && a.Path == n.Path))
            .Select(a => new PermissionData(a.ClusterName, a.PermissionKey, a.Path, a.Propagated, a.BuiltIn))
            .ToList();

        return (toRemove, toAdd);
    }

    public async Task SyncForUserAsync(string userId, IEnumerable<UserPermission> newPermissions)
    {
        var (toRemove, toAdd) = CalcPermissionsDiff(await GetUserPermissionsAsync(userId), newPermissions);
        if (toRemove.Count != 0) { await RemoveForUserAsync(userId, toRemove); }
        if (toAdd.Count != 0) { await AddForUserAsync(userId, toAdd); }
    }

    public async Task SyncForRoleAsync(string roleId, IEnumerable<RolePermission> newPermissions)
    {
        var (toRemove, toAdd) = CalcPermissionsDiff(await GetRolePermissionsAsync(roleId), newPermissions);
        if (toRemove.Count != 0) { await RemoveForRoleAsync(roleId, toRemove); }
        if (toAdd.Count != 0) { await AddForRoleAsync(roleId, toAdd); }
    }

    public async Task SyncForAppTokenAsync(Guid appTokenId, IEnumerable<AppTokenPermission> newPermissions)
    {
        var (toRemove, toAdd) = CalcPermissionsDiff(await GetAppTokenPermissionsAsync(appTokenId), newPermissions);
        if (toRemove.Count != 0) { await RemoveForAppTokenAsync(appTokenId, toRemove); }
        if (toAdd.Count != 0) { await AddForAppTokenAsync(appTokenId, toAdd); }
    }

    public ValueTask ClearCacheAsync() => fusionCache.RemoveByTagAsync(CacheTag);
}
