/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Hangfire.Client;
using Hangfire.Server;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.BackgroundJobs.Filters;

public class UserContextJobFilter(IServiceProvider serviceProvider) : IClientFilter, IServerFilter
{
    private const string ParameterName = "UserId";

    public void OnCreating(CreatingContext context)
    {
        using var scope = serviceProvider.CreateScope();
        var current = scope.ServiceProvider.GetService<ICurrentUserService>();

        var userId = current?.UserId;
        if (!string.IsNullOrEmpty(userId)) { context.SetJobParameter(ParameterName, userId); }
    }

    public void OnCreated(CreatedContext context)
    {

    }

    public void OnPerforming(PerformingContext context)
    {
        var userId = context.GetJobParameter<string>(ParameterName);

        var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // No HTTP user at schedule time (cron, hosted service): run as system.
        var effectiveId = string.IsNullOrEmpty(userId)
                            ? SystemUser.Id
                            : userId;
        var user = userManager.FindByIdAsync(effectiveId).GetAwaiter().GetResult();

        if (user is null)
        {
            scope.Dispose();
            return;
        }

        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
        var principal = signInManager.CreateUserPrincipalAsync(user).GetAwaiter().GetResult();

        var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            User = principal,
            RequestServices = scope.ServiceProvider,
        };

        // Keep the scope alive for the duration of the job; disposed in OnPerformed.
        context.Items["UserContextScope"] = scope;
    }

    public void OnPerformed(PerformedContext context)
    {
        if (context.Items.TryGetValue("UserContextScope", out var scopeObj) && scopeObj is IServiceScope scope)
        {
            scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext = null;
            scope.Dispose();
            context.Items.Remove("UserContextScope");
        }
    }
}
