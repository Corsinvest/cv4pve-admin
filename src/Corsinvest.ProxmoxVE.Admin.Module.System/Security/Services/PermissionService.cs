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

    public async Task<bool> HasAsync(string clusterName, string permissionKey, string path)
        => await HasAsync(currentUserService.UserId, clusterName, permissionKey, path);

    public async Task<bool> HasAsync(string userId, string clusterName, string permissionKey, string path)
    {
        if (string.IsNullOrEmpty(userId)) { return false; }

        var userPermissionsTask = GetUserPermissionsAsync(userId);
        var userRolesTask = GetUserRolesAsync(userId);
        await Task.WhenAll(userPermissionsTask, userRolesTask);

        var userPermissions = await userPermissionsTask;
        var userRoles = await userRolesTask;

        // Check direct user permissions (highest priority)
        if (userPermissions.Any(a => a.HasPermission(permissionKey, path, clusterName))) { return true; }

        // Check role permissions in parallel
        if (userRoles.Count != 0)
        {
            var rolePermissionTasks = userRoles.Select(GetRolePermissionsAsync);
            var rolesClaimsTask = GetRolesPermissionsAsync();

            var allRolePermissions = await Task.WhenAll(rolePermissionTasks);
            var rolesClaims = await rolesClaimsTask;

            // Check scoped role permissions
            foreach (var rolePermissions in allRolePermissions)
            {
                if (rolePermissions.Any(a => a.HasPermission(permissionKey, path, clusterName)))
                {
                    return true;
                }
            }

            // Check global role claims
            foreach (var roleId in userRoles)
            {
                if (rolesClaims.TryGetValue(roleId, out var claims) && claims.Contains(permissionKey))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private async Task<Dictionary<string, List<string>>> GetRolesPermissionsAsync() => await fusionCache.GetOrSetAsync("Permissions:RolesClaims", async token =>
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
                                                                                            }, TimeSpan.FromMinutes(10), tags: [CacheTag]);

    public async Task<List<UserPermission>> GetUserPermissionsAsync(string userId) => await fusionCache.GetOrSetAsync($"Permissions:UserId:{userId}", async token =>
                                                                                           {
                                                                                               await using var db = await dbContextFactory.CreateDbContextAsync(token);

                                                                                               return await db.UserPermissions.AsNoTracking()
                                                                                                                              .Where(a => a.UserId == userId)
                                                                                                                              .ToListAsync(token);
                                                                                           }, TimeSpan.FromMinutes(10), tags: [CacheTag]);

    private async Task<List<string>> GetUserRolesAsync(string userId) => await fusionCache.GetOrSetAsync($"Permissions:UserRoles:{userId}", async token =>
                                                                              {
                                                                                  await using var db = await dbContextFactory.CreateDbContextAsync(token);

                                                                                  return await db.UserRoles.AsNoTracking()
                                                                                                           .Where(a => a.UserId == userId)
                                                                                                           .Select(a => a.RoleId)
                                                                                                           .ToListAsync(token);
                                                                              }, TimeSpan.FromMinutes(10), tags: [CacheTag]);

    public async Task<List<RolePermission>> GetRolePermissionsAsync(string roleId) => await fusionCache.GetOrSetAsync($"Permissions:RolePermissions:{roleId}", async token =>
                                                                                           {
                                                                                               await using var db = await dbContextFactory.CreateDbContextAsync(token);

                                                                                               return await db.RolePermissions.AsNoTracking()
                                                                                                                              .Where(a => a.RoleId == roleId)
                                                                                                                              .ToListAsync(token);
                                                                                           }, TimeSpan.FromMinutes(10), tags: [CacheTag]);

    public async Task<bool> HasAnyAsync(string userId, string clusterName, IEnumerable<string> permissionKeys, string path)
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

    public async Task AddForUserAsync(string userId,
                                      string clusterName,
                                      string permissionKey,
                                      string path,
                                      bool propagated,
                                      bool builtIn)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("UserId cannot be null or empty", nameof(userId));
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();

        if (!await db.Users.AnyAsync(a => a.Id == userId))
        {
            throw new InvalidOperationException($"User with ID {userId} does not exist");
        }

        var userPermission = await db.UserPermissions
                                     .FirstOrDefaultAsync(a => a.UserId == userId
                                                               && a.ClusterName == clusterName
                                                               && a.PermissionKey == permissionKey
                                                               && a.Path == path);

        if (userPermission == null)
        {
            userPermission = new UserPermission
            {
                UserId = userId,
                ClusterName = clusterName,
                PermissionKey = permissionKey,
                Path = path,
                Propagated = propagated,
                BuiltIn = builtIn
            };
            db.UserPermissions.Add(userPermission);
        }
        else
        {
            userPermission.Propagated = propagated;
            userPermission.BuiltIn = builtIn;
        }

        await db.SaveChangesAsync();
        await InvalidateUserPermissionsAsync(userId);
    }

    public async Task RemoveForUserAsync(string userId, string clusterName, string permissionKey, string path)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();

        var query = db.UserPermissions.Where(up => up.UserId == userId);
        query = query.Where(a => a.ClusterName == clusterName, !string.IsNullOrEmpty(clusterName));
        query = query.Where(a => a.PermissionKey == permissionKey, !string.IsNullOrEmpty(permissionKey));
        query = query.Where(a => a.Path == path, !string.IsNullOrEmpty(path));

        await query.ExecuteDeleteAsync(CancellationToken.None);
        await InvalidateUserPermissionsAsync(userId);
    }

    public async Task RemoveForUserAsync(string userId, IEnumerable<(string clusterName, string permissionKey, string path)> permissions)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();

        foreach (var (clusterName, permissionKey, path) in permissions)
        {
            var query = db.UserPermissions.Where(up => up.UserId == userId);
            query = query.Where(a => a.ClusterName == clusterName, !string.IsNullOrEmpty(clusterName));
            query = query.Where(a => a.PermissionKey == permissionKey, !string.IsNullOrEmpty(permissionKey));
            query = query.Where(a => a.Path == path, !string.IsNullOrEmpty(path));

            await query.ExecuteDeleteAsync(CancellationToken.None);
        }

        await InvalidateUserPermissionsAsync(userId);
    }

    public async Task AddForRoleAsync(string roleId,
                                      string clusterName,
                                      string permissionKey,
                                      string path,
                                      bool propagated,
                                      bool builtIn)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();

        if (!await db.Roles.AnyAsync(r => r.Id == roleId))
        {
            throw new InvalidOperationException($"Role with ID {roleId} does not exist");
        }

        var rolePermission = await db.RolePermissions
                                     .FirstOrDefaultAsync(a => a.RoleId == roleId &&
                                                               a.ClusterName == clusterName &&
                                                               a.PermissionKey == permissionKey &&
                                                               a.Path == path);

        if (rolePermission == null)
        {
            rolePermission = new RolePermission
            {
                RoleId = roleId,
                ClusterName = clusterName,
                PermissionKey = permissionKey,
                Path = path,
                Propagated = propagated,
                BuiltIn = builtIn
            };
            db.RolePermissions.Add(rolePermission);
        }
        else
        {
            rolePermission.Propagated = propagated;
            rolePermission.BuiltIn = builtIn;
        }

        await db.SaveChangesAsync();
        await InvalidateRolePermissionsAsync(roleId);
        await InvalidateUsersWithRoleAsync(roleId);
    }

    public async Task RemoveForRoleAsync(string roleId, string clusterName, string permissionKey, string path)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();

        var query = db.RolePermissions.Where(a => a.RoleId == roleId);
        query = query.Where(rp => rp.ClusterName == clusterName, !string.IsNullOrEmpty(clusterName));
        query = query.Where(rp => rp.PermissionKey == permissionKey, !string.IsNullOrEmpty(permissionKey));
        query = query.Where(rp => rp.Path == path, !string.IsNullOrEmpty(path));

        await query.ExecuteDeleteAsync(CancellationToken.None);
        await InvalidateRolePermissionsAsync(roleId);
        await InvalidateUsersWithRoleAsync(roleId);
    }

    public async Task RemoveForRoleAsync(string roleId, IEnumerable<(string clusterName, string permissionKey, string path)> permissions)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();

        foreach (var (clusterName, permissionKey, path) in permissions)
        {
            var query = db.RolePermissions.Where(a => a.RoleId == roleId);
            query = query.Where(rp => rp.ClusterName == clusterName, !string.IsNullOrEmpty(clusterName));
            query = query.Where(rp => rp.PermissionKey == permissionKey, !string.IsNullOrEmpty(permissionKey));
            query = query.Where(rp => rp.Path == path, !string.IsNullOrEmpty(path));

            await query.ExecuteDeleteAsync(CancellationToken.None);
        }

        await InvalidateRolePermissionsAsync(roleId);
        await InvalidateUsersWithRoleAsync(roleId);
    }

    public async Task AddForRoleAsync(string roleId,
                                      IEnumerable<(string clusterName, string permissionKey, string path, bool propagated, bool builtIn)> permissions)
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
        var newPermissions = permissionsList.Where(a => !existingSet.Contains($"{a.clusterName}|{a.permissionKey}|{a.path}"))
                                            .Select(a => new RolePermission
                                            {
                                                RoleId = roleId,
                                                ClusterName = a.clusterName,
                                                PermissionKey = a.permissionKey,
                                                Path = a.path,
                                                Propagated = a.propagated,
                                                BuiltIn = a.builtIn
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

    public async Task AddForUserAsync(string userId,
                                      IEnumerable<(string clusterName, string permissionKey, string path, bool propagated, bool builtIn)> permissions)
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
        var newPermissions = permissionsList.Where(p => !existingSet.Contains($"{p.clusterName}|{p.permissionKey}|{p.path}"))
                                            .Select(a => new UserPermission
                                            {
                                                UserId = userId,
                                                ClusterName = a.clusterName,
                                                PermissionKey = a.permissionKey,
                                                Path = a.path,
                                                Propagated = a.propagated,
                                                BuiltIn = a.builtIn
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

        foreach (var userId in usersWithRole)
        {
            await fusionCache.RemoveAsync($"Permissions:UserId:{userId}");
            await fusionCache.RemoveAsync($"Permissions:UserRoles:{userId}");
        }
    }

    private async Task InvalidateRolePermissionsAsync(string roleId)
    {
        await fusionCache.RemoveAsync($"Permissions:RolePermissions:{roleId}");
        await fusionCache.RemoveAsync("Permissions:RolesClaims");
    }

    public ValueTask ClearCacheAsync() => fusionCache.RemoveByTagAsync(CacheTag);
}
