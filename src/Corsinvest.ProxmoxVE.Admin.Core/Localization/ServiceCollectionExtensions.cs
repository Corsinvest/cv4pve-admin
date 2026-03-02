/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;

namespace Corsinvest.ProxmoxVE.Admin.Core.Localization;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLocalization(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.SetDefaultCulture(ApplicationHelper.DefaultCulture)
                   .AddSupportedCultures(ApplicationHelper.SupportedCultures)
                   .AddSupportedUICultures(ApplicationHelper.SupportedCultures);
            options.ApplyCurrentCultureToResponseHeaders = true;
            options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider
            {
                CookieName = ApplicationHelper.CookieCultureName
            });
        });

        services.AddSingleton<JsonLocalizationService>();
        services.AddSingleton<IStringLocalizerFactory, JsonLocalizerFactory>();
        services.AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));
        services.AddTransient<IStringLocalizer>(sp => sp.GetRequiredService<JsonLocalizationService>());

        return services;
    }
}
