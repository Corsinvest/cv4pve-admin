/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Auditing.Domains.Entities;
using Corsinvest.AppHero.Core.Domain.Repository;
using Corsinvest.AppHero.Core.Extensions;
using Corsinvest.AppHero.Core.Helpers;
using Corsinvest.AppHero.Core.MudBlazorUI.Style;
using Corsinvest.AppHero.Core.Options;
using Corsinvest.ProxmoxVE.Admin.Core.UI.Layout;
using Corsinvest.ProxmoxVE.Admin.Persistence;
using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Identity = Corsinvest.AppHero.Core.Security.Identity;

namespace Corsinvest.ProxmoxVE.Admin;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Customize(this IServiceCollection services)
    {
        _ = new[] { typeof(System.Web.HttpUtility) };

        var writableAppOptions = services.GetWritableOptions<AppOptions>();
        writableAppOptions.Value.Name = "cv4pve-admin";
        writableAppOptions.Value.Author = "Corsinvest Srl";
        writableAppOptions.Value.Icon = "<g transform=\"matrix(0.004684, 0, 0, -0.004684, 0.008197, 23.984467)\" fill=\"#8c33b5ff\" stroke=\"none\" data-bx-origin=\"0 5.206446\"><path d=\"M404 5105 c-191 -41 -349 -201 -390 -394 -21 -99 -21 -4203 0 -4302 42 -195 200 -353 395 -395 99 -21 4203 -21 4302 0 195 42 353 200 395 395 21 99 21 4203 0 4302 -42 195 -200 353 -395 395 -95 20 -4214 19 -4307 -1z m2970 -1980 c37 -18 10 26 278 -439 120 -209 223 -384 227 -388 3 -4 112 176 241 400 214 370 238 409 272 425 49 24 101 9 129 -37 18 -30 19 -56 19 -558 l0 -528 -105 0 -105 0 -2 326 -3 326 -175 -308 c-96 -169 -183 -318 -195 -332 -13 -15 -35 -25 -62 -29 -38 -5 -46 -2 -75 28 -19 19 -110 168 -203 332 l-170 299 -3 -321 -2 -321 -105 0 -105 0 0 533 c0 588 -3 564 64 593 40 17 42 17 80 -1z m-1728 -39 l34 -34 0 -526 0 -526 -105 0 -105 0 0 135 0 135 -350 0 -349 0 -3 -132 -3 -133 -102 -3 -103 -3 0 320 c0 193 4 341 11 373 20 95 76 197 148 268 77 76 141 113 243 140 63 16 111 19 362 19 l288 1 34 -34z m1026 23 c158 -34 291 -155 339 -308 24 -80 34 -262 20 -381 -21 -177 -106 -302 -258 -377 l-77 -38 -350 -3 c-193 -2 -361 0 -374 3 -12 3 -31 20 -42 38 -19 31 -20 53 -20 521 l0 488 34 34 34 34 321 0 c199 0 340 -4 373 -11z\"></path><path d=\"M989 2887 c-81 -28 -132 -65 -170 -125 -39 -62 -49 -102 -49 -199 l0 -83 350 0 350 0 0 215 0 215 -207 0 c-189 0 -214 -3 -274 -23z\"></path><path d=\"M2120 2554 l0 -356 258 4 c292 4 315 9 383 85 56 62 64 96 64 278 0 152 -1 162 -26 210 -29 56 -91 109 -147 125 -21 5 -145 10 -284 10 l-248 0 0 -356z\"></path></g>";
        writableAppOptions.Value.Url = "https://www.corsinvest.it/cv4pve-admin";
        writableAppOptions.Value.RepoGitHub = "corsinvest/cv4pve-admin";
        writableAppOptions.Value.RepoDockerHub = "corsinvest/cv4pve-admin";
        writableAppOptions.Update(writableAppOptions.Value);

        var writableUIOptions = services.GetWritableOptions<UIOptions>();
        writableUIOptions.Value.ClassIndexPageComponent = TypeHelper.GetClassAndAssemblyName<PveWidgets>();
        writableUIOptions.Value.Theme.PrimaryColor = "#8c33b5ff";
        writableUIOptions.Update(writableUIOptions.Value);

        return services;
    }

    private static IServiceCollection ConfigureHangfire(this IServiceCollection services)
    {
        services.AddHangfireServer();
        GlobalConfiguration.Configuration.UseStorage(new SQLiteStorage(Path.Combine(ApplicationHelper.PathData, "hangfire.db")));
        GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 1 });
        return services;
    }

    private static IServiceCollection ConfigureIdentity(this IServiceCollection services)
    {
        //set options
        services.AddIdentity<Identity.ApplicationUser, Identity.ApplicationRole>(options => services.GetOptionsSnapshot<Identity.Options>()!.Value.SetIdentityOptions(options))
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

        return services;
    }

    private static IServiceCollection ConfigureDbApp(this IServiceCollection services)
        => services.AddDbContext<ApplicationDbContext>(builder =>
        {
#if DEBUG
            builder.EnableDetailedErrors();
            builder.EnableSensitiveDataLogging();
#endif
            builder.UseSqlite(DataBaseHelper.CreateSQLitePath(Path.Combine(ApplicationHelper.PathData, "app.db")));
        })
        .AddRepository(typeof(ApplicationDbContext), typeof(AuditTrail));

    public static IServiceCollection ConfigureApp(this IServiceCollection services)
        => services.ConfigureHangfire()
                   .ConfigureDbApp()
                   .ConfigureIdentity();
}