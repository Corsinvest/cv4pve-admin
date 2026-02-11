namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Services;

internal class CurrentUserService(IHttpContextAccessor httpContextAccessor,
                                  IServiceScopeFactory scopeFactory) : ICurrentUserService
{
    public async Task<ApplicationUser?> GetUserAsync()
    {
        if (!string.IsNullOrEmpty(UserId))
        {
            using var scope = scopeFactory.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByIdAsync(UserId);
        }
        else
        {
            return null;
        }
    }

    public ClaimsPrincipal? ClaimsPrincipal => httpContextAccessor.HttpContext?.User;

    public string UserId
        => IsAuthenticated
            ? ClaimsPrincipal!.FindFirst(ClaimTypes.NameIdentifier)!.Value
            : string.Empty;

    public string UserName
        => IsAuthenticated
            ? ClaimsPrincipal!.FindFirst(ClaimTypes.Name)!.Value
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
