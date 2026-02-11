using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.HealthMonitoring;

internal static class ServiceCollectionExtensions
{
    //public static void UseHealthChecksAdmin(this IApplicationBuilder app)
    //{
    //    // app.MapHealthChecks()
    //    //app.Use(async (context, next) =>
    //    //{
    //    //    //var path = context.Request.Path;
    //    //    //if (path.StartsWithSegments("/health"))
    //    //    //{
    //    //    //    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
    //    //    //                                        .CreateLogger("HealthChecksMiddleware");

    //    //    //    var settingsService = context.RequestServices.GetRequiredService<ISettingsService>();
    //    //    //    var settings = settingsService.GetForModule<Module, Settings>(ApplicationHelper.AllClusterName);

    //    //    //    if (string.IsNullOrEmpty(settings.HealthChecksToken))
    //    //    //    {
    //    //    //        settings.HealthChecksToken = Guid.NewGuid().ToString();
    //    //    //        var modularityService = context.RequestServices.GetRequiredService<IModularityService>();
    //    //    //        await settingsService.SetAsync(modularityService.Get<Module>()!, string.Empty, settings);
    //    //    //    }

    //    //    //    var token = context.Request.Query["token"].ToString();

    //    //    //    if (path != "/health" && (string.IsNullOrEmpty(token) || token != settings.HealthChecksToken))
    //    //    //    {
    //    //    //        if (!context.Response.HasStarted)
    //    //    //        {
    //    //    //            context.Response.StatusCode = StatusCodes.Status403Forbidden;
    //    //    //            await context.Response.WriteAsync(JsonSerializer.Serialize(new
    //    //    //            {
    //    //    //                error = "Forbidden: Invalid token",
    //    //    //                status = 403,
    //    //    //            }));
    //    //    //        }
    //    //    //        else
    //    //    //        {
    //    //    //            logger.LogWarning("The response has already started, the error response cannot be sent.");
    //    //    //        }
    //    //    //        return;
    //    //    //    }
    //    //    //}

    //    //    await next();
    //    //});
    //}

    public static IServiceCollection AddHealthChecksAdmin(this IServiceCollection services)
    {
        services.AddHealthChecks()
                .AddCheck("live_check", () => HealthCheckResult.Healthy("Alive and running"), ["live"]);

        //.AddCheck("ready_check",
        //          () => ApplicationHelper.IsReady
        //                ? HealthCheckResult.Healthy("Ready to serve traffic")
        //                : HealthCheckResult.Unhealthy("Not ready yet"),
        //          ["ready"])

        //.AddCheck("startup_check",
        //          () => ApplicationHelper.IsStartupComplete
        //                ? HealthCheckResult.Healthy("Startup complete")
        //                : HealthCheckResult.Degraded("Still starting up"),
        //          ["startup"])

        return services;
    }

    private static Task WriteHealthChecksResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e =>
            new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        });

        return context.Response.WriteAsync(result);
    }

    public static IEndpointRouteBuilder MapHealthChecksAdmin(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = WriteHealthChecksResponse
        });

        return endpoints;

        //endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        //{
        //    Predicate = (check) => check.Tags.Contains("live"),
        //    ResponseWriter = WriteHealthChecksResponse
        //});

        //endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        //{
        //    Predicate = (check) => check.Tags.Contains("ready"),
        //    ResponseWriter = WriteHealthChecksResponse
        //});

        //endpoints.MapHealthChecks("/health/startup", new HealthCheckOptions
        //{
        //    Predicate = (check) => check.Tags.Contains("startup"),
        //    ResponseWriter = WriteHealthChecksResponse
        //});
    }
}
