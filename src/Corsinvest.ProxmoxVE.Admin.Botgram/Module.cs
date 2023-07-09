/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Botgram.Components;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Corsinvest.ProxmoxVE.Admin.Botgram;

public class Module : PveAdminModuleBase, IForceLoadModule
{
    public Module()
    {
        Authors = "Corsinvest Srl";
        Company = "Corsinvest Srl";
        Keywords = "Customer,Portal";
        Description = "Telegram Bot";
        InfoText = "Configure your Telegram with this bot to manage your Proxmox VE cluster remotely";
        SetCategory(AdminModuleCategory.Control);

        Link = new ModuleLink(this, Description)
        {
            Icon = Icons.Custom.Brands.Telegram,
            Render = typeof(RenderIndex)
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

        UrlHelp += "#chapter_module_botgram";
    }

    public override void ConfigureServices(IServiceCollection services, IConfiguration config)
        => AddOptions<Options, RenderOptions>(services, config)
            .AddHostedService<BotgramService>();

    public override async Task RefreshOptionsAsync(IServiceScope scope)
        => await scope.ServiceProvider.GetServices<IHostedService>()
                                      .OfType<BotgramService>()
                                      .FirstOrDefault()!
                                      .Restart();
}