using System.Security.Claims;

namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    Task<ApplicationUser?> GetUserAsync();
    ClaimsPrincipal? ClaimsPrincipal { get; }
    string UserId { get; }
    string UserName { get; }
    string IpAddress { get; }
    string HttpConnectionId { get; }
    string CircuitId { get; }
}
