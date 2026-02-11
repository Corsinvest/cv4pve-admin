using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
using Hangfire.Dashboard;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.BackgroundJobs.Filters;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        if (httpContext.User.Identity is { IsAuthenticated: false }) { return false; }

        var permissionService = httpContext.RequestServices.GetService<IPermissionService>()!;
        return permissionService.HasAsync(ApplicationHelper.AllClusterName, Permissions.Dashboard)
                                        .GetAwaiter()
                                        .GetResult();
    }
}
