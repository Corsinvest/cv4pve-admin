/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Core.Persistence;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Services;
using Microsoft.Extensions.Configuration;

namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic;

public class Module : ModuleBase
{
    public Module()
    {
        Keywords = "diagnostic,health,troubleshoot,scan,issues,errors,analysis,cluster health,compliance,iso27001,nis2,dora,pci dss,audit";
        ModuleType = ModuleType.Application;
        Name = "Diagnostic";
        Description = "Automated cluster health checks, diagnostics, issue detection and compliance checks";
        Category = Categories.Health;
        Slug = "diagnostic";
        HelpUrl = "modules/diagnostics";

        NavBar =
        [
            new(this,"Overview", string.Empty)
            {
                Render = new(typeof(Components.Overview)),
                Icon = PveAdminUIHelper.Icons.Overview
            },
            new (this, "Scans")
            {
                Render = new(typeof(Components.Scans)),
                Icon = PveAdminUIHelper.Icons.Scans
            },
            new(this,"Ignored Issues")
            {
                Render = new(typeof(Components.Issues)),
                Icon = "block"
            }
        ];

        Link = new(this, Name, string.Empty)
        {
            Icon = "stethoscope",
            Render = NavBar.ToList()[0].Render
        };

        Widgets =
        [
            new(this,"Status")
            {
                Description = "Diagnostic Status",
                RenderInfo = new(typeof(Components.Widgets.Status)),
                Width = 3,
                Height = 5
            },
            new(this,"Check")
            {
                Description = "Diagnostic Issues Check",
                RenderInfo = new(typeof(Components.Widgets.Check)),
                Width = 3,
                Height = 5
            }
        ];

        Roles =
        [
            new(Permissions.Scan.Data.Permissions
                                     .CombineWith(Permissions.Scan.Run)
                                     .CombineWith(Permissions.Scan.Delete)
                                     .CombineWith(Permissions.IgnoredIssue.Data))
        ];
    }

    protected override string PermissionBaseKey => Permissions.BaseName;

    public static class Permissions
    {
        public static string BaseName { get; } = "Diagnostic";

        public static class Scan
        {
            public static PermissionsRead Data { get; } = new(BaseName, nameof(Scan), nameof(Data));
            public static Permission Run { get; } = new(Data.Prefix, nameof(Run), "Scan");
            public static Permission Delete { get; } = new(Data.Prefix, nameof(Delete), "Delete");
        }

        public static class IgnoredIssue
        {
            public static PermissionsCrud Data { get; } = new(BaseName, nameof(IgnoredIssue), nameof(Data));
        }
    }

    /// <summary>
    /// Component used to render the Compliance tab inside scan details.
    /// CE returns a stub that shows a "Get Enterprise" gate. EE overrides this
    /// to return the real grouped grid.
    /// </summary>
    public virtual Type ComplianceViewType => typeof(Components.ComplianceView);

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        => AddSettings<Settings, Components.RenderSettings>(services)
            .AddDbContextFactoryPostgreSql<ModuleDbContext>("diagnostic")
            .AddScoped<IDiagnosticService, DiagnosticService>();

    protected override async Task RefreshSettingsAsync(IServiceScope scope)
    {
        await scope.GetEventNotificationService().PublishAsync(new DataChangedNotification());
        InitializeJob(scope);
    }

    public override IModuleMaintenance GetMaintenance(IServiceScope scope)
        => new PostgreSqlModuleMaintenance<ModuleDbContext>(scope);

    public override Task FixAsync(IServiceScope scope) => RunAsync(scope);

    protected override async Task RunAsync(IServiceScope scope)
    {
        await scope.MigrateDbAsync<ModuleDbContext>();
        InitializeJob(scope);
    }

    private static void InitializeJob(IServiceScope scope)
    {
        var backgroundJobService = scope.GetBackgroundJobService();
        var settingsService = scope.GetSettingsService();

        foreach (var item in settingsService.GetEnabledClustersSettings().Select(a => a.Name))
        {
            var settings = settingsService.GetForModule<Module, Settings>(item);
            backgroundJobService.ScheduleOrRemove<Job>(a => a.ScanAsync(settings.ClusterName),
                                             settings.CronExpression,
                                             settings.Enabled,
                                             settings.ClusterName);
        }
    }
}
