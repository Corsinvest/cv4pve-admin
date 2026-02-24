/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Security.Cryptography;
using System.Text;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.AppTokens;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Services;

public class AppTokenService(IDbContextFactory<ModuleDbContext> dbContextFactory,
                             IAuditService auditService) : IAppTokenService
{
    public async Task<(string RawToken, AppToken Token)> GenerateAsync(string name, string? ownerId, DateTime? expiresAt)
    {
        var rawToken = GenerateRawToken();
        var hash = HashToken(rawToken);

        var token = new AppToken
        {
            Id = Guid.NewGuid(),
            Name = name,
            TokenHash = hash,
            OwnerId = ownerId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt
        };

        await using var db = await dbContextFactory.CreateDbContextAsync();
        db.AppTokens.Add(token);
        await db.SaveChangesAsync();

        await auditService.LogAsync("AppTokens.Create", true, $"Token: {token.Name}");

        return (rawToken, token);
    }

    public async Task<AppToken?> ValidateAsync(string rawToken)
    {
        var hash = HashToken(rawToken);

        await using var db = await dbContextFactory.CreateDbContextAsync();
        return await db.AppTokens
                       .AsNoTracking()
                       .FirstOrDefaultAsync(a => a.TokenHash == hash
                                                    && a.IsActive
                                                    && (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow));
    }

    public async Task RevokeAsync(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        await db.AppTokens
                .Where(a => a.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsActive, false));
    }

    public async Task<string> RegenerateAsync(Guid id)
    {
        var rawToken = GenerateRawToken();
        var hash = HashToken(rawToken);

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var name = await db.AppTokens
                           .Where(a => a.Id == id)
                           .Select(a => a.Name)
                           .FirstOrDefaultAsync();

        await db.AppTokens
                .Where(a => a.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.TokenHash, hash));

        await auditService.LogAsync("AppTokens.Regenerate", true, $"Token: {name}");

        return rawToken;
    }

    public async Task DeleteAsync(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var name = await db.AppTokens
                           .Where(a => a.Id == id)
                           .Select(a => a.Name)
                           .FirstOrDefaultAsync();

        await db.AppTokens
                .Where(a => a.Id == id)
                .ExecuteDeleteAsync();

        await auditService.LogAsync("AppTokens.Delete", true, $"Token: {name}");
    }

    public async Task<bool> UpdateAsync(Guid id, string name, string? ownerId, bool isActive, DateTime? expiresAt)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var token = await db.AppTokens.FirstOrDefaultAsync(a => a.Id == id);
        if (token == null) { return false; }

        token.Name = name;
        token.OwnerId = ownerId;
        token.IsActive = isActive;
        token.ExpiresAt = expiresAt;
        await db.SaveChangesAsync();

        await auditService.LogAsync("AppTokens.Update", true, $"Token: {token.Name}");

        return true;
    }

    public async Task<List<AppToken>> GetAllAsync()
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        return await db.AppTokens
                       .AsNoTracking()
                       .Include(a => a.Owner)
                       .Include(a => a.AppTokenRoles).ThenInclude(r => r.Role)
                       .OrderBy(a => a.Name)
                       .ToListAsync();
    }

    public async Task<List<AppToken>> GetByPermissionKeysAsync(IEnumerable<string> permissionKeys)
    {
        var keys = permissionKeys.ToList();
        await using var db = await dbContextFactory.CreateDbContextAsync();
        return await db.AppTokens
                       .AsNoTracking()
                       .Include(a => a.Owner)
                       .Include(a => a.AppTokenRoles).ThenInclude(r => r.Role).ThenInclude(r => r.RolePermissions)
                       .Include(a => a.AppTokenPermissions)
                       .Where(a => a.AppTokenPermissions.Any(p => keys.Contains(p.PermissionKey))
                                || a.AppTokenRoles.Any(r => r.Role.RolePermissions.Any(p => keys.Contains(p.PermissionKey))))
                       .OrderBy(a => a.Name)
                       .ToListAsync();
    }

    public async Task<AppToken?> GetByName(string name)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        return await db.AppTokens
                       .AsNoTracking()
                       .Include(a => a.Owner)
                       .Include(a => a.AppTokenRoles).ThenInclude(r => r.Role)
                       .Include(a => a.AppTokenPermissions)
                       .FirstOrDefaultAsync(a => a.Name == name);
    }

    public async Task<AppToken?> GetByIdAsync(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        return await db.AppTokens
                       .AsNoTracking()
                       .Include(a => a.Owner)
                       .Include(a => a.AppTokenRoles).ThenInclude(r => r.Role)
                       .Include(a => a.AppTokenPermissions)
                       .FirstOrDefaultAsync(a => a.Id == id);
    }

    public static string HashToken(string rawToken)
        => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

    private static string GenerateRawToken()
        => $"cvat_{Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                          .Replace("+", "-")
                          .Replace("/", "_")
                          .TrimEnd('=')}";

    public async Task SetRolesAsync(Guid id, IEnumerable<string> roles)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var roleIds = await db.Roles
                              .Where(a => roles.Contains(a.Name!))
                              .Select(a => a.Id)
                              .ToListAsync();

        await db.AppTokenRoles.Where(a => a.AppTokenId == id).ExecuteDeleteAsync();
        db.AppTokenRoles.AddRange(roleIds.Select(roleId => new AppTokenRole
        {
            AppTokenId = id,
            RoleId = roleId
        }));
        await db.SaveChangesAsync();
    }

    public async Task SyncRolesAsync(Guid id, IEnumerable<string> roles)
    {
        var rolesList = roles.ToList();
        await using var db = await dbContextFactory.CreateDbContextAsync();

        var desiredRoleIds = await db.Roles
                                     .Where(a => rolesList.Contains(a.Name!))
                                     .Select(a => a.Id)
                                     .ToListAsync();

        var existingRoleIds = await db.AppTokenRoles
                                      .Where(a => a.AppTokenId == id)
                                      .Select(a => a.RoleId)
                                      .ToListAsync();

        var toAdd = desiredRoleIds.Except(existingRoleIds).ToList();
        var toRemove = existingRoleIds.Except(desiredRoleIds).ToList();

        if (toRemove.Count > 0)
        {
            await db.AppTokenRoles
                    .Where(a => a.AppTokenId == id && toRemove.Contains(a.RoleId))
                    .ExecuteDeleteAsync();
        }

        if (toAdd.Count > 0)
        {
            db.AppTokenRoles.AddRange(toAdd.Select(roleId => new AppTokenRole
            {
                AppTokenId = id,
                RoleId = roleId
            }));
            await db.SaveChangesAsync();
        }
    }
}
