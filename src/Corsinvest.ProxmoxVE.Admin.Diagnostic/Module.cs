/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Diagnostic.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Corsinvest.ProxmoxVE.Admin.Diagnostic;

public class Module : PveAdminModuleBase, IForceLoadModule
{
    public Module()
    {
        Authors = "Corsinvest Srl";
        Company = "Corsinvest Srl";
        Keywords = "Diagnostic,Fix,Error";
        Description = "Diagnostic";
        InfoText = "Take, schedule and manage automatic diagnostic for your Proxmox VE cluster, it helps to find out some hidden problems";
        SetCategory(AdminModuleCategory.Health);

        Link = new ModuleLink(this, Description)
        {
            Icon = "fa-solid fa-stethoscope",
            Render = typeof(RenderIndex)
        };

        Roles = new Role[]
        {
            new("",
                "",
                Permissions.Issue.Data.Permissions
                    .Union(Permissions.Result.Data.Permissions)
                    .Union(new []
                    {
                        Permissions.Result.Run ,
                        Permissions.Result.Delete,
                        Permissions.Result.DownloadPdf,
                    }))
        };

        Widgets = new[]
        {
            new ModuleWidget(this,"Status")
            {
                GroupName = Category,
                Render = typeof(RenderWidget),
                Class = "mud-grid-item mud-grid-item-xs-12 mud-grid-item-sm-6 mud-grid-item-md-4 mud-grid-item-lg-4"
            }
        };

        UrlHelp += "#chapter_module_diagnostic";
    }

    public override void ConfigureServices(IServiceCollection services, IConfiguration config)
        => AddOptions<Options, RenderOptions>(services, config)
            .AddDbContext<DiagnosticDbContext>(options => options.UseSqlite($"Data Source={Path.Combine(PathData, "db.db")}"))
            .AddRepositories<DiagnosticDbContext>(new[] { typeof(Execution), typeof(ExecutionData), typeof(IgnoredIssue) });

    public override async Task OnApplicationInitializationAsync(IHost host)
    {
        await Task.CompletedTask;
        host.DatabaseMigrate<DiagnosticDbContext>();
    }

    public class Permissions
    {
        public class Issue
        {
            public static PermissionsCrud Data { get; } = new($"{typeof(Module).FullName}.{nameof(Issue)}.{nameof(Data)}");
        }

        public class Result
        {
            public static PermissionsRead Data { get; } = new($"{typeof(Module).FullName}.{nameof(Result)}.{nameof(Data)}");
            public static Permission Run { get; } = new($"{Data.Prefix}.{nameof(Run)}", "Run", Icons.Material.Filled.PlayArrow, UIColor.Success);
            public static Permission Delete { get; } = new($"{Data.Prefix}.{nameof(Delete)}", "Delete", Icons.Material.Filled.DeleteForever, UIColor.Error);
            public static Permission DownloadPdf { get; } = new($"{Data.Prefix}.{nameof(DownloadPdf)}", "Download PDF", Icons.Custom.FileFormats.FilePdf, UIColor.Error);
        }
    }
}
