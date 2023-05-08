/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Corsinvest.AppHero.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Corsinvest.ProxmoxVE.Admin.Core;

public class ModuleCore : ModuleBase, IForceLoadModule
{
    public ModuleCore()
    {
        Authors = "Corsinvest Srl";
        Company = "Corsinvest Srl";
        Keywords = "ProxmoxVE,Admin,Core";
        Category = IModularityService.AdministrationCategoryName;
        Type = ModuleType.Service;
        Description = "ProxmoxVE Admin Core";
    }

    public override void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddBlazoredSessionStorage();
        services.AddBlazoredLocalStorage();
    }


    public override async Task OnPreApplicationInitializationAsync(IHost host)
    {
        await Task.CompletedTask;
        var modularityService = host.Services.GetRequiredService<IModularityService>();

        //add icon 
        foreach (var item in Enum.GetValues<ModuleCategory>())
        {
            modularityService.SetCategoryIcon(PveAdminHelper.GetCategoryName(item), PveAdminHelper.GetCategoryIcon(item));
        }
    }
}
