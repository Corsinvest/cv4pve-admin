/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security;

// The tickets are in the database, with a short copy in memory in front of it: the database is read again
// after a restart, and at most every CacheFor for a session in use.
internal class DbTicketStore(IDbContextFactory<ModuleDbContext> dbContextFactory,
                             IFusionCache fusionCache,
                             IDataProtectionProvider dataProtectionProvider,
                             IOptionsMonitor<CookieAuthenticationOptions> cookieOptions) : ITicketStore, ISessionStore
{
    // Short, so that a session removed by another process, as the command line, is closed soon
    private static readonly TimeSpan CacheFor = TimeSpan.FromMinutes(2);

    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("Corsinvest.ProxmoxVE.Admin.SessionTicket");

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = $"TicketStore:{Guid.NewGuid()}";
        var now = DateTime.UtcNow;

        await using var db = await dbContextFactory.CreateDbContextAsync();
        db.UserSessions.Add(new()
        {
            Key = key,
            UserId = GetUserId(ticket),
            CreatedAt = now,
            ExpiresAt = GetExpiry(ticket),
            Ticket = Protect(ticket)
        });

        // The sessions nobody came back to
        await db.UserSessions.Where(a => a.ExpiresAt < now).ExecuteDeleteAsync();
        await db.SaveChangesAsync();

        await CacheAsync(key, ticket);
        return key;
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var expiresAt = GetExpiry(ticket);
        var data = Protect(ticket);

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var updated = await db.UserSessions.Where(a => a.Key == key)
                                           .ExecuteUpdateAsync(a => a.SetProperty(b => b.ExpiresAt, expiresAt)
                                                                      .SetProperty(b => b.Ticket, data));

        // A sign-in made by a request that is already signed in keeps its key: when the sessions of the user were
        // just closed, as after a change of password, the row is gone and this is the session that stays
        if (updated == 0)
        {
            db.UserSessions.Add(new()
            {
                Key = key,
                UserId = GetUserId(ticket),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                Ticket = data
            });
            await db.SaveChangesAsync();
        }

        await CacheAsync(key, ticket);
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var cached = await fusionCache.TryGetAsync<AuthenticationTicket>(key);
        if (cached.HasValue) { return cached.Value; }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var now = DateTime.UtcNow;
        var data = await db.UserSessions.Where(a => a.Key == key && a.ExpiresAt > now)
                                        .Select(a => a.Ticket)
                                        .FirstOrDefaultAsync();
        if (data == null) { return null; }

        AuthenticationTicket? ticket;
        try
        {
            ticket = TicketSerializer.Default.Deserialize(_protector.Unprotect(data));
        }
        catch (global::System.Security.Cryptography.CryptographicException)
        {
            // Written with keys that are gone: the session cannot be read any more
            ticket = null;
        }

        if (ticket != null) { await CacheAsync(key, ticket); }
        return ticket;
    }

    public async Task RemoveAsync(string key)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        await db.UserSessions.Where(a => a.Key == key).ExecuteDeleteAsync();
        await fusionCache.RemoveAsync(key);
    }

    public async Task RemoveForUserAsync(string userId)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var keys = await db.UserSessions.Where(a => a.UserId == userId).Select(a => a.Key).ToListAsync();
        if (keys.Count == 0) { return; }

        await db.UserSessions.Where(a => a.UserId == userId).ExecuteDeleteAsync();
        foreach (var key in keys) { await fusionCache.RemoveAsync(key); }
    }

    private byte[] Protect(AuthenticationTicket ticket) => _protector.Protect(TicketSerializer.Default.Serialize(ticket));

    private static string GetUserId(AuthenticationTicket ticket)
        => ticket.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidOperationException("The ticket has no user id");

    private async Task CacheAsync(string key, AuthenticationTicket ticket)
    {
        var left = GetExpiry(ticket) - DateTime.UtcNow;
        if (left > TimeSpan.Zero) { await fusionCache.SetAsync(key, ticket, left < CacheFor ? left : CacheFor); }
    }

    // The expiry written in the ticket is the one of the cookie that points to it
    private DateTime GetExpiry(AuthenticationTicket ticket)
        => ticket.Properties.ExpiresUtc is { } expires && expires > DateTimeOffset.UtcNow
            ? expires.UtcDateTime
            : DateTime.UtcNow + cookieOptions.Get(IdentityConstants.ApplicationScheme).ExpireTimeSpan;
}
