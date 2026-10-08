/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security;

internal class FusionCacheTicketStore(IFusionCache fusionCache,
                                      IOptionsMonitor<CookieAuthenticationOptions> cookieOptions) : ITicketStore
{
    public async Task RemoveAsync(string key) => await fusionCache.RemoveAsync(key);

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
        => await fusionCache.SetAsync(key, ticket, GetDuration(ticket));

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var result = await fusionCache.TryGetAsync<AuthenticationTicket>(key);
        return result.HasValue ? result.Value : null;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = $"TicketStore:{Guid.NewGuid()}";
        await fusionCache.SetAsync(key, ticket, GetDuration(ticket));
        return key;
    }

    // The expiry written in the ticket is the one of the cookie that points to it
    private TimeSpan GetDuration(AuthenticationTicket ticket)
        => ticket.Properties.ExpiresUtc is { } expires && expires > DateTimeOffset.UtcNow
            ? expires - DateTimeOffset.UtcNow
            : cookieOptions.Get(IdentityConstants.ApplicationScheme).ExpireTimeSpan;
}
