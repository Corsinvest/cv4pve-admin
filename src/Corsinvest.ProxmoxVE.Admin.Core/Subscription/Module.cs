/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Modularity;
using Corsinvest.AppHero.Core.Security.Auth.Permissions;
using Corsinvest.AppHero.Core.UI;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Corsinvest.ProxmoxVE.Admin.Core.Subscription;

public class Module : PveAdminModuleBase, IForceLoadModule
{
    public Module()
    {
        Authors = "Corsinvest Srl";
        Company = "Corsinvest Srl";
        Keywords = "Subscription";
        SetCategory(AdminModuleCategory.Support);
        Icon = Icons.Material.Filled.Support;
        Type = ModuleType.Application;
        Description = "Subscription";
        Slug = "Subscription";

        Link = new ModuleLink(this, Description)
        {
            Icon = Icons.Material.Filled.Support,
            Render = typeof(RenderIndex),
            IconColor = UIColor.Primary,
        };

        Roles = new Role[]
        {
            new("",
                "",
                Permissions.Subscription.Data.Permissions.Union(new []
                {
                    Permissions.Subscription.Check,
                    Permissions.Subscription.Register,
                }))
        };
    }

    public override void ConfigureServices(IServiceCollection services, IConfiguration config) => services.AddScoped<SubscriptionService>();

    public class Permissions
    {
        public class Subscription
        {
            public static PermissionsCrud Data { get; } = new($"{typeof(Module).FullName}.{nameof(Subscription)}.{nameof(Data)}");
            public static Permission Check { get; } = new($"{Data.Prefix}.{nameof(Check)}", "Check", Icons.Material.Filled.Check, UIColor.Info);
            public static Permission Register { get; } = new($"{Data.Prefix}.{nameof(Register)}", "Register", Icons.Material.Filled.AppRegistration, UIColor.Info);
        }
    }
}