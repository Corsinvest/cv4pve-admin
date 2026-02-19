/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Persistence;
using Microsoft.Extensions.Configuration;

namespace Corsinvest.ProxmoxVE.Admin.Module.Dashboard;

public class Module : ModuleBase
{
    public Module()
    {
        Keywords = "dashboard,widgets,monitoring,overview,customizable,charts,metrics";
        ModuleType = ModuleType.Application;
        Name = "Dashboard";
        Description = "Customizable dashboards with widgets and metrics";
        Slug = "dashboard";
        Icon = "dashboard";
        Scope = ClusterScope.All;

        Link = new(this, Name, string.Empty)
        {
            Icon = Icon,
            Render = new(typeof(Components.Dashboard)),
            OrderIndex = -1
        };

        Widgets =
        [
            new(this, "Web Content")
            {
                Description = "Link, Iframe or HTML content",
                RenderInfo = new(typeof(Components.Widgets.WebContent.Render)),
                RenderSettingsInfo = new(typeof(Components.Widgets.WebContent.Settings),
                                         typeof(Components.Widgets.WebContent.RenderSettings))
            },
            new(this, "Memo")
            {
                Description = "Markdown notes",
                RenderInfo = new(typeof(Components.Widgets.Memo.Render)),
                RenderSettingsInfo = new(typeof(Components.Widgets.Memo.Settings),
                                         typeof(Components.Widgets.Memo.RenderSettings)),
                Width = 4,
                Height = 4
            }
        ];
    }

    protected override string PermissionBaseKey { get; } = "Dashboard";

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        => services.AddDbContextFactoryPostgreSql<ModuleDbContext>("dashboard");

    public override Task DatabaseMaintenanceAsync(IServiceScope scope, DatabaseMaintenanceOperation operation)
        => scope.GetRequiredService<ModuleDbContext>().ExecuteMaintenanceAsync(operation);

    public override Task FixAsync(IServiceScope scope) => RunAsync(scope);

    protected override async Task RunAsync(IServiceScope scope)
        => await scope.MigrateDbAsync<ModuleDbContext>();
}
