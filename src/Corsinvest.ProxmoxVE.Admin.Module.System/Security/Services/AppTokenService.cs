/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Security.Cryptography;
using System.Text;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.AppTokens;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Services;

public class AppTokenService(IDbContextFactory<ModuleDbContext> dbContextFactory) : IAppTokenService
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

        return (rawToken, token);
    }

    public async Task<AppToken?> ValidateAsync(string rawToken)
    {
        var hash = HashToken(rawToken);

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var token = await db.AppTokens
                            .AsNoTracking()
                            .FirstOrDefaultAsync(a => a.TokenHash == hash);

        return token is { IsValid: true } ? token : null;
    }

    public async Task RevokeAsync(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        await db.AppTokens
                .Where(a => a.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsActive, false));
    }

    public async Task DeleteAsync(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        await db.AppTokens
                .Where(a => a.Id == id)
                .ExecuteDeleteAsync();
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
}
