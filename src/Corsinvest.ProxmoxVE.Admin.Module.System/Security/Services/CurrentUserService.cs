/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Services;

internal class CurrentUserService(IHttpContextAccessor httpContextAccessor,
                                  IServiceScopeFactory scopeFactory) : ICurrentUserService
{
    public async Task<ApplicationUser?> GetUserAsync()
    {
        if (!string.IsNullOrEmpty(UserId))
        {
            using var scope = scopeFactory.CreateScope();
            return await scope.GetRequiredService<UserManager<ApplicationUser>>().FindByIdAsync(UserId);
        }
        else
        {
            return null;
        }
    }

    public ClaimsPrincipal? ClaimsPrincipal => httpContextAccessor.HttpContext?.User;

    // An app token is authenticated and is not a user: it has no user id, and its name is the name of the token
    public string UserId
        => IsAuthenticated
            ? ClaimsPrincipal!.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty
            : string.Empty;

    public string UserName
        => IsAuthenticated
            ? ClaimsPrincipal!.FindFirst(ClaimTypes.Name)?.Value ?? "anonymous"
            : "anonymous";

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated is true;
    public string HttpConnectionId => httpContextAccessor.HttpContext?.Connection.Id!;
    public string CircuitId => httpContextAccessor.HttpContext?.Items["CircuitId"]?.ToString() ?? string.Empty;

    public string IpAddress
    {
        get
        {
            var ip = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress + string.Empty;
            return string.IsNullOrEmpty(ip) || string.IsNullOrWhiteSpace(ip)
                    ? "unknown"
                    : ip;
        }
    }
}
