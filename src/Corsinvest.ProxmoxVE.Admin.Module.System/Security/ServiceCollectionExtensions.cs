/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Threading.RateLimiting;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Admin.Module.System.Security.Components;
using Corsinvest.ProxmoxVE.Admin.Module.System.Security.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSecurityAdmin(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEmailSender<ApplicationUser>, EmailSenderService>();

        services.AddCascadingAuthenticationState();
        services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
        services.AddSingleton<ITicketStore, FusionCacheTicketStore>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
        .AddIdentityCookies();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Login";
            options.LogoutPath = "/Logout";
            options.AccessDeniedPath = "/AccessDenied";

            // Cookie security settings
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(configuration.GetValue("CookieSettings:ExpireDays", 14));
            options.SessionStore = services.BuildServiceProvider().GetRequiredService<ITicketStore>();
        });

        services.AddAuthorization();

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedAccount = true;
            options.SignIn.RequireConfirmedEmail = true;

            //read from settings
            var sectionPasswordOptions = configuration.GetSection("Security:PasswordOptions");
            if (sectionPasswordOptions.Exists())
            {
                var passwordOptions = sectionPasswordOptions.Get<PasswordOptions>()!;
                options.Password.RequiredLength = passwordOptions.RequiredLength;
                options.Password.RequireNonAlphanumeric = passwordOptions.RequireNonAlphanumeric;
                options.Password.RequireDigit = passwordOptions.RequireDigit;
                options.Password.RequireLowercase = passwordOptions.RequireLowercase;
                options.Password.RequireUppercase = passwordOptions.RequireUppercase;
            }

            var sectionLockoutOptions = configuration.GetSection("Security:LockoutOptions");
            if (sectionLockoutOptions.Exists())
            {
                var lockoutOptions = sectionLockoutOptions.Get<LockoutOptions>()!;
                options.Lockout.MaxFailedAccessAttempts = lockoutOptions.MaxFailedAccessAttempts;
                options.Lockout.AllowedForNewUsers = lockoutOptions.AllowedForNewUsers;
                options.Lockout.DefaultLockoutTimeSpan = lockoutOptions.DefaultLockoutTimeSpan == TimeSpan.Zero
                                                            ? TimeSpan.FromMinutes(15)
                                                            : lockoutOptions.DefaultLockoutTimeSpan;
            }

            // 2FA settings
            options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddRoles<ApplicationRole>()
        .AddEntityFrameworkStores<ModuleDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

        // Rate limiting
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                httpContext => RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User?.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1)
                    }));
        });

        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }

    public static async Task InitializeSecurityAsync(this IServiceScope scope) => await PopulateSecurityAsync(scope.ServiceProvider);
    public static void MapSecurityAdmin(this WebApplication app) => app.MapAdditionalIdentityEndpoints();

    private static async Task PopulateSecurityAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(Module));
        logger.LogDebug("Initialize Security db");

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        const string email = "admin@local";
        var adminUser = await userManager.FindByEmailAsync(email);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = email,
                IsActive = true,
                DisplayName = "Admin",
                Email = email,
                EmailConfirmed = true,
                BuiltIn = true
            };

            //default password
            await userManager.CreateAsync(adminUser, "Password123!");
        }

        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        await roleManager.CreateAsync(RoleConstants.AdministratorRole, "Admin", true, false);
        await userManager.AddToRolesAsync(adminUser, [RoleConstants.AdministratorRole]);

        var permissionService = services.GetRequiredService<IPermissionService>();

        var roles = new[]
        {
            ClusterPermissions.RoleAdmin,
            ClusterPermissions.RoleNodeUser,
            ClusterPermissions.RoleStorageUser,
            ClusterPermissions.RoleVmUser,
            ApplicationPermissions.Role
        };
        foreach (var item in roles)
        {
            var role = await roleManager.CreateAsync(item, permissionService);
            await userManager.AddToRoleAsync(adminUser, role.Name!);

            if (ClusterPermissions.RoleAdmin == item)
            {
                await permissionService.AddForRoleAsync(role.Id,
                                                        item.Permissions.Select(a => (ApplicationHelper.AllClusterName, a.Key, "*", true, true)));
            }
        }

        //create roles for module
        foreach (var module in services.GetRequiredService<IModuleService>().Modules)
        {
            await userManager.AddToRoleAsync(adminUser, (await roleManager.CreateAsync(module.RoleAdmin, permissionService)).Name!);

            //role and permission specific module
            foreach (var item in module.AllRoles)
            {
                await roleManager.CreateAsync(item, permissionService);
            }
        }
    }
}
