/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Core.Persistence;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Persistence;
using Microsoft.Extensions.Configuration;

namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport;

public class Module : ModuleBase
{
    public Module()
    {
        Keywords = "report,system,analysis,cluster,export,pdf,documentation,audit,overview,vm,node,storage";
        ModuleType = ModuleType.Application;
        Name = "System Report";
        Description = "Generate comprehensive cluster, VM, node and storage reports";
        Category = Categories.Utilities;
        Slug = "system-report";
        HelpUrl = "modules/system-report";

        NavBar =
        [
            new(this,"Overview",string.Empty)
            {
                Render = new(typeof(Components.Overview)),
                Icon = PveAdminUIHelper.Icons.Overview
            },
            new(this,"Reports")
            {
                Render = new(typeof(Components.Reports)),
                Icon = PveAdminUIHelper.Icons.Scans
            }
        ];

        Link = new(this, Name, string.Empty)
        {
            Icon = "description",
            Render = NavBar.ToList()[0].Render
        };

        Roles =
        [
            new(Permissions.Report.Data.Permissions
                                       .CombineWith(Permissions.Report.Download))
        ];

        Directory.CreateDirectory(PathData);
    }

    protected override string PermissionBaseKey => Permissions.BaseName;

    public static class Permissions
    {
        public static string BaseName { get; } = "SystemReport";

        public static class Report
        {
            public static PermissionsCrud Data { get; } = new(BaseName, nameof(Report), nameof(Data));
            public static Permission Download { get; } = new(Data.Prefix, nameof(Download), "Download");
        }
    }

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
       => services.AddDbContextFactoryPostgreSql<ModuleDbContext>("system_reports");

    public override IModuleMaintenance GetMaintenance(IServiceScope scope)
        => new PostgreSqlModuleMaintenance<ModuleDbContext>(scope);

    public override Task FixAsync(IServiceScope scope) => RunAsync(scope);

    protected override Task RunAsync(IServiceScope scope)
        => scope.MigrateDbAsync<ModuleDbContext>();
}
