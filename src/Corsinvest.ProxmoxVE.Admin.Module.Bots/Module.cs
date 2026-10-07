/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
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
        HelpUrl = "modules/bots";

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

        Roles = [new([Permissions.Chat.SendMessage])];
    }

    protected override string PermissionBaseKey => Permissions.BaseName;

    public static class Permissions
    {
        public static string BaseName { get; } = "Bots";

        public static class Chat
        {
            public static Permission SendMessage { get; } = new($"{BaseName}.{nameof(Chat)}", nameof(SendMessage), "Send message");
        }
    }

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        => AddSettings<Settings, Components.RenderSettings>(services)
            .AddHostedService<BotgramService>();

    protected override Task RefreshSettingsAsync(IServiceScope scope)
        => scope.ServiceProvider.GetServices<IHostedService>()
                                .OfType<BotgramService>()
                                .FirstOrDefault()!
                                .RestartAsync();
}
