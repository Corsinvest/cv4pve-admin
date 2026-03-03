/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Configuration;
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Core.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Module.Profile.Session;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Corsinvest.ProxmoxVE.Admin.Module.Profile;

public class Module : ModuleBase
{
    public Module()
    {
        Keywords = "profile,account,password,2fa,authentication,sessions,user,security";
        ModuleType = ModuleType.Application;
        Name = "Profile";
        Description = "User profile, password and security settings";
        Slug = "profile";
        HelpUrl = "configuration/profile";
        Icon = "account_circle";
        LinkPosition = ModuleLinkPosition.ProfileMenu;
        Scope = ClusterScope.All;

        var passwordLink = new ModuleLinkBase(this, "Password")
        {
            Render = new(typeof(Components.ChangePassword)),
            Icon = "lock"
        };

        UrlHelper.UrlChangePassword = $"{BaseUrl}/{passwordLink.Url}";

        NavBar =
        [
            new(this,"Overview", string.Empty)
            {
                Render = new(typeof(Components.Overview)),
                Icon = PveAdminUIHelper.Icons.Overview
            },
            new(this,"Preferences", "preferences")
            {
                Icon = "tune",
                Render = SettingsHelper.CreateSettingsAccordion<UserSettings>([new()
                {
                    Title = "Language",
                    Description = "Select your preferred language for the user interface",
                    Icon = "translate",
                    ComponentType = typeof(Components.CulturePreferences),
                    OnSavedAsync = OnSavedPreferencesAsync
                }],true),
            },

            //new(this,"Profile")
            //{
            ////                Render = typeof(Components.AppSettings),
            //},
            //new(this,"Account")
            //{
            //    Render = new(typeof(Components.Account)),
            //},
            passwordLink,
            new(this,"Two-factor authentication (2FA)","2fa")
            {
                Render = new(GetTwoFactorAuthenticationType()),
                Icon = "security"
            },
            //new(this,"Passkeys","passkeys")
            //{
            //    Render = new(GetPasskeysAuthenticationType())
            //},
            //new(this,"Access tokens")
            //{
            //    //Render = typeof(Components.AppSettings),
            //},
            //new(this,"Preferences")
            //{
            //    //Render = typeof(Components.NotificationsSettings),
            //},
            new(this,"Audit Logs")
            {
                Render = new(GetAuditLogsType()),
                Icon = "history"
            },
            new(this,"Active Sessions")
            {
                Render = new(typeof(Components.ActiveSessions)),
                Icon = "play_circle"
            }
        ];

        Link = new(this, Name, string.Empty)
        {
            Render = NavBar.ToList()[0].Render,
            Icon = "account_circle"
        };

        Roles =
        [
            new Role(PermissionBaseKey,
                     "Profile User",
                     true,
                     true,
                     NavBarPermissions)
        ];
    }

    private async Task OnSavedPreferencesAsync(IServiceScope scope, UserSettings settings)
    {
        var navigationManager = scope.GetRequiredService<NavigationManager>();
        navigationManager.NavigateTo($"/set-culture?culture={settings.Culture}&redirectUri={navigationManager.Uri}", forceLoad: true);
    }

    protected override string PermissionBaseKey { get; } = "Profile";

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDetection();
        services.AddSingleton<CircuitHandler, SessionInfoCircuitHandler>();
        services.AddSingleton<ISessionsInfoTracker>(sp => (SessionInfoCircuitHandler)sp.GetRequiredService<CircuitHandler>());
    }

    protected override void Map(WebApplication app)
    {
        app.UseDetection();

        app.MapGet("/set-culture", (string culture, string redirectUri, HttpContext context) =>
        {
            if (culture != null) { context.Response.AppendCultureCookie(culture); }
            return Results.LocalRedirect(redirectUri ?? "/");
        }).RequireAuthorization();
    }

    protected virtual Type GetTwoFactorAuthenticationType() => typeof(Core.Components.SubscriptionRequired);
    protected virtual Type GetPasskeysAuthenticationType() => typeof(Core.Components.SubscriptionRequired);
    protected virtual Type GetAuditLogsType() => typeof(Core.Components.SubscriptionRequired);
}
