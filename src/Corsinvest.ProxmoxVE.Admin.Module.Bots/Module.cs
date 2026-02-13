/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Module.Bots.Telegram;
using Microsoft.Extensions.Configuration;

namespace Corsinvest.ProxmoxVE.Admin.Module.Bots;

public class Module : ModuleBase
{
    public Module()
    {
        Keywords = "bot,telegram,chatbot,remote,mobile,notifications,commands,messaging";
        ModuleType = ModuleType.Application;
        Category = Categories.Control;
        Name = "Bots";
        Slug = "bots";
        Description = "Remote cluster management via Telegram chatbot";

        NavBar =
        [
            new(this,"Overview", string.Empty)
            {
                Render = new(typeof(Components.Overview)),
                Icon = PveAdminUIHelper.Icons.Overview
            },
            new(this,"Telegram")
            {
                Icon = "send",
                Render = new(typeof(Telegram.Components.Render))
            }
        ];

        Link = new(this, Name, string.Empty)
        {
            Icon = "smart_toy",
            Render = NavBar.ToList()[0].Render
        };
    }

    protected override string PermissionBaseKey { get; } = "Bots";

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        => AddSettings<Settings, Components.RenderSettings>(services)
            .AddHostedService<BotgramService>();

    protected override async Task RefreshSettingsAsync(IServiceScope scope)
        => await scope.ServiceProvider.GetServices<IHostedService>()
                                      .OfType<BotgramService>()
                                      .FirstOrDefault()!
                                      .RestartAsync();
}
