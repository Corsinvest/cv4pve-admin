/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Persistence;
using Corsinvest.ProxmoxVE.Admin.Module.System.BackgroundJobs;
using Corsinvest.ProxmoxVE.Admin.Module.System.HealthMonitoring;
using Corsinvest.ProxmoxVE.Admin.Module.System.ReleaseChecker;
using Corsinvest.ProxmoxVE.Admin.Module.System.Security;
using Corsinvest.ProxmoxVE.Admin.Module.System.Settings;
using Corsinvest.ProxmoxVE.Admin.Module.System.SystemLogs.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Corsinvest.ProxmoxVE.Admin.Module.System;

public class Module : ModuleBase
{
    protected ModuleLinkBase PveClusterLink { get; }

    public Module()
    {
        Keywords = "system,admin,configuration,security,users,monitoring,settings,clusters";
        ModuleType = ModuleType.Application;
        Name = "Admin Area";
        Description = "System administration, security, monitoring and Proxmox VE cluster configuration";
        Slug = "system";
        Icon = "build";
        Scope = ClusterScope.All;
        LinkPosition = ModuleLinkPosition.ProfileMenu;

        PveClusterLink = new ModuleLinkBase(this, "Proxmox VE Clusters")
        {
            Icon = "dns",
            Render = new(typeof(Components.ClusterConfig.RenderClustersSettings))
        };

        ApplicationHelper.UrlNewPveConfig = $"{BaseUrl}/{PveClusterLink.Url}?new=true";

        CreateNavMenu();

        Link = new(this, Name, string.Empty)
        {
            Render = NavBar.ToList()[0].Render,
            Icon = "build"
        };

        Roles = [new(Security.Permissions.Users.Data.Permissions
                             .CombineWith(Security.Permissions.Users.ResetPassword)
                             .CombineWith(Security.Permissions.Users.Permissions)
                             .CombineWith(Security.Permissions.Roles.Data)
                             .CombineWith(Security.Permissions.Roles.Permissions)
                             .CombineWith(BackgroundJobs.Permissions.Dashboard))];
    }

    protected override string PermissionBaseKey { get; } = "System";

    protected virtual void CreateNavMenu() => NavBar =
        [
            new(this,"Overview", string.Empty)
            {
                Render = new(typeof(Components.Overview)),
                Icon = PveAdminUIHelper.Icons.Overview
            },

            PveClusterLink,

            new(this, "Security")
            {
                Icon = "security",
                Child =
                [
                    new(this,"Users","security/users")
                    {
                        Render = new(typeof(Core.Components.SubscriptionRequired)),
                        Icon = "person"
                    },
                    new(this,"Roles")
                    {
                        Render = new(typeof(Core.Components.SubscriptionRequired)),
                        Icon = "theater_comedy"
                    },
                    new(this,"Audit Logs")
                    {
                        Render = new(typeof(Core.Components.SubscriptionRequired)),
                        Icon = "history"
                    },
                    new(this,"Active Sessions")
                    {
                        Render = new (typeof(Core.Components.ActiveSessions)),
                        Icon = "play_circle"
                    },
                ]
            },

            new(this,"Monitoring")
            {
                Icon = "monitoring",
                Child =
                [
                    new(this,"System information")
                    {
                        Render = new (typeof(Components.Monitoring.SystemInfo)),
                        Icon = "info"
                    },

                    new (this, "Background Jobs")
                    {
                        Render = new (typeof(BackgroundJobs.Components.Dashboard)),
                        Icon = "schedule"
                    },

                    new(this,"Logs")
                    {
                        Render = new (typeof(Core.Components.SubscriptionRequired)),
                        Icon = "description"
                    }
                ]
            },

            new(this, "Maintenance")
            {
                Icon = "build_circle",
                Render = new(typeof(Components.Maintenance))
            },

            new(this,"Settings")
            {
                Icon = "settings",
                Child =
                [
                    new(this,"General")
                    {
                        Icon = "settings_applications",
                        Render = CreateSettingsAccordion([GetSectionSMTPConfiguration()])
                    }
                ]
            },

            new(this,"Notifications")
            {
                Render = new (typeof(Components.NotifierSettings)),
                Icon = "notifications"
            },

            new(this,"Support")
            {
                Render = new (typeof(Components.Support)),
                Icon = "support_agent"
            },
        ];

    protected static RenderComponentInfo CreateSettingsAccordion(IEnumerable<Components.SettingSection> sections)
        => new(typeof(Components.SettingsAccordion),
               new Dictionary<string, object> { [nameof(Components.SettingsAccordion.Sections)] = sections });

    protected static Components.SettingSection GetSectionSMTPConfiguration()
        => new()
        {
            Title = "SMTP Configuration",
            Description = "Configure SMTP server settings for email notifications",
            Icon = "email",
            ComponentType = typeof(Components.SmtpConfig)
        };

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        => services.AddDbContextFactoryPostgreSql<ModuleDbContext>("system")
                   .AddHealthChecksAdmin()
                   .AddSettingsAdmin()
                   .AddSecurityAdmin(configuration)
                   .AddBackgroundJobsAdmin(configuration)
                   .AddReleaseServices(configuration)
                   .AddScoped<ISystemLogService, SystemLogService>();

    public override Task DatabaseMaintenanceAsync(IServiceScope scope, DatabaseMaintenanceOperation operation)
        => scope.GetRequiredService<ModuleDbContext>().ExecuteMaintenanceAsync(operation);

    public override Task FixAsync(IServiceScope scope) => RunAsync(scope);

    protected override async Task RunAsync(IServiceScope scope)
    {
        await scope.MigrateDbAsync<ModuleDbContext>();
        await scope.InitializeSecurityAsync();
    }

    protected override void Map(WebApplication app)
    {
        app.MapHealthChecksAdmin();
        app.MapSecurityAdmin();
        app.MapBackgroundJobsAdmin();
    }
}
