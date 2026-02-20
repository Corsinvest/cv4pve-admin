/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Extensions;
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Core.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Metrics.Exporter.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Corsinvest.ProxmoxVE.Admin.Module.MetricsExporter;

public class Module : ModuleBase
{
    internal static IDictionary<string, Info> Infos { get; } = new Dictionary<string, Info>();
    private static string ExporterUrl { get; set; } = string.Empty;

    public Module()
    {
        Keywords = "metrics,prometheus,exporter,monitoring,performance,statistics,observability";
        ModuleType = ModuleType.Application;
        Scope = ClusterScope.All;
        Name = "Metrics Exporter";
        Description = "Prometheus metrics exporter for cluster monitoring and observability";
        Category = Categories.Health;
        Slug = "metrics-exporter";
        HelpUrl = "modules/metrics-exporter";

        if (string.IsNullOrEmpty(ExporterUrl))
        {
            ExporterUrl = $"{BaseUrl}/prometheus";
        }

        NavBar =
        [
            new(this,"Overview",string.Empty)
            {
                Render = new(typeof(Components.Overview)),
                Icon = PveAdminUIHelper.Icons.Overview
            },
            new(this,"Status")
            {
                Render = new(typeof(Components.Status)),
                Icon = PveAdminUIHelper.Icons.Status
            }
        ];

        Link = new(this, Name, string.Empty)
        {
            Icon = "multiline_chart",
            Render = NavBar.ToList()[0].Render
        };
    }

    protected override string PermissionBaseKey { get; } = "MetricsExporter";

    internal static string GetUrl(string clusterName) => $"{ExporterUrl}/{clusterName}";

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        => AddSettings<Settings, Components.RenderSettings>(services);

    protected override void Map(WebApplication app) => MapExporterPrometheusMetrics(app);

    private static void MapExporterPrometheusMetrics(WebApplication app)
        => app.MapGet(ExporterUrl + "/{clusterName}", async (string clusterName,
                                                             HttpContext context,
                                                             ILogger<Module> logger,
                                                             IServiceScopeFactory scopeFactory) =>
        {
            var settingsService = scopeFactory.CreateScope().GetSettingsService();
            if (settingsService.GetEnabledClustersSettings().Any(a => a.Name == clusterName))
            {
                var settings = settingsService.GetForModule<Module, Settings>(ApplicationHelper.AllClusterName);
                if (!settings.Prometheus.Enabled)
                {
                    context.Response.StatusCode = 503;
                    return Results.Problem("Metrics Exporter is disabled");
                }

                if (string.IsNullOrWhiteSpace(settings.Prometheus.Token))
                {
                    return Results.BadRequest("Token in setting not configured!");
                }

                var token = context.Request.Query["token"].ToString();
                if (!string.Equals(token, settings.Prometheus.Token, StringComparison.Ordinal)) { return Results.Unauthorized(); }

                if (!Infos.TryGetValue(clusterName, out var info))
                {
                    //create register
                    var registry = Prometheus.Metrics.NewCustomRegistry();
                    registry.AddBeforeCollectCallback(async () =>
                    {
                        var scope = scopeFactory.CreateScope();
                        var settingsService = scope.GetSettingsService();
                        var adminService = scope.GetAdminService();

                        var settings = settingsService.GetForModule<Module, Settings>(clusterName);
                        var exporter = new PrometheusExporter(registry, settings.Prometheus.ExporterPrefix);

                        try
                        {
                            await exporter.CollectAsync(await adminService[clusterName].GetPveClientAsync());
                        }
                        catch (Exception ex) { logger.LogError(ex, ex.Message); }
                    });

                    //register info
                    info = new() { Registry = registry };
                    Infos.Add(clusterName, info);
                }

                //update statistic
                info.LastRequest = DateTime.Now;
                if (long.MaxValue == info.CountRequest) { info.CountRequest = 0; }
                info.CountRequest++;

                //execute and return data
                await using var ms = new MemoryStream();
                await info.Registry.CollectAndExportAsTextAsync(ms);
                ms.Position = 0;
                using var sr = new StreamReader(ms);
                return Results.Text(sr.ReadToEnd(), "text/plain");
            }
            else
            {
                return Results.BadRequest("Cluster not enabled");
            }
        });
}
