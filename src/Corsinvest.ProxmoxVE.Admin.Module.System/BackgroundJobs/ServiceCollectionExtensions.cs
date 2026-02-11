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
        services.AddHangfire(config =>
        {
            config.UsePostgreSqlStorage(options => options.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection")),
                                                                               new PostgreSqlStorageOptions { SchemaName = "hangfire" });

            config.UseFilter(new LogJobFilter());
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
