/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Module.System.BackgroundJobs.Filters;
using Corsinvest.ProxmoxVE.Admin.Module.System.BackgroundJobs.Services;
using Hangfire;
using Hangfire.Console.Extensions;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.BackgroundJobs;

internal static class ServiceCollectionExtensions
{
    public static string BackgroundJobsDashboardUrl { get; } = "/background-jobs-internal";

    //internal static Permission DashboardPermission { get; } = new(Permissions.BaseName, "Dashboard", "Show dashboard Hangfire");

    //protected override string PermissionBaseKey => Permissions.BaseName;

    public static IServiceCollection AddBackgroundJobsAdmin(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IBackgroundJobService, BackgroundJobService>();

        services.AddHangfireConsoleExtensions();

        GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });
        //GlobalJobFilters.Filters.Add(new DisableConcurrentExecutionAttribute(60) { });
        GlobalJobFilters.Filters.Add(new PreventConcurrentExecutionWithJobAndArgsFilter());

        //services.AddSingleton<JobActivator, JobActivatorEx>();
        services.AddHangfire((sp, config) =>
        {
            config.UsePostgreSqlStorage(options => options.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection")),
                                                                               new PostgreSqlStorageOptions { SchemaName = "hangfire" });

            // UserContextJobFilter must run before LogJobFilter so the log
            // entries see the principal populated by the former.
            config.UseFilter(new UserContextJobFilter(sp));
            config.UseFilter(new LogJobFilter(sp));
            // config.UseFilter(new SkipConcurrentExecutionFilter(provider.GetRequiredService<ILogger<SkipConcurrentExecutionFilter>>()));
            config.UseColouredConsoleLogProvider();
            config.UseRecommendedSerializerSettings();
            config.UseSerilogLogProvider();
        });

        services.AddHangfireServer();

        return services;
    }

    public static void MapBackgroundJobsAdmin(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var appSettings = scope.GetRequiredService<ISettingsService>().GetAppSettings();

        // Fallback: if AddHangfire lambda was bypassed (e.g. Elsa starts its
        // own Hangfire server before our config runs), our filters never get
        // registered. Add them here against the live application provider.
        var logger = scope.GetRequiredService<ILoggerFactory>().CreateLogger("HangfireConfig");
        if (!GlobalJobFilters.Filters.Select(f => f.Instance).OfType<UserContextJobFilter>().Any())
        {
            logger.LogWarning("UserContextJobFilter not registered — adding via fallback on app.Services");
            GlobalJobFilters.Filters.Add(new UserContextJobFilter(app.Services));
            GlobalJobFilters.Filters.Add(new LogJobFilter(app.Services));
        }
        else
        {
            logger.LogInformation("UserContextJobFilter already registered via AddHangfire lambda");
        }

        app.MapHangfireDashboard(BackgroundJobsDashboardUrl,
                                 new DashboardOptions
                                 {
                                     DarkModeEnabled = true,
                                     AppPath = null,
                                     DashboardTitle = $"{appSettings.AppName} Jobs",
                                     DisplayStorageConnectionString = false,
                                     Authorization = [new HangfireDashboardAuthorizationFilter()]
                                 });
    }
}
