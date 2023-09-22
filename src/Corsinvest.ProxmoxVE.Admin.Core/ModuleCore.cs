/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Corsinvest.AppHero.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuestPDF.Infrastructure;

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
        QuestPDF.Settings.License = LicenseType.Community;
        services.AddBlazoredSessionStorage();
        services.AddBlazoredLocalStorage();
    }

    public override async Task OnPreApplicationInitializationAsync(IHost host)
    {
        await Task.CompletedTask;
        var modularityService = host.Services.GetRequiredService<IModularityService>();

        //add icon 
        foreach (var (_, category) in PveAdminHelper.ModuleCategories)
        {
            var cat = modularityService.Categories.FirstOrDefault(a => a.Name == category.Name);
            if (cat == null)
            {
                modularityService.Categories.Add(category);
            }
            else
            {
                cat.Icon = category.Icon;
            }
        }
    }
}
