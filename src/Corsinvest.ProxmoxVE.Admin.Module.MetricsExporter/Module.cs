/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Corsinvest.ProxmoxVE.Admin.Core.Extensions;
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PrometheusMetricsEngine = Corsinvest.ProxmoxVE.Metrics.Exporter.Api.Prometheus.MetricsEngine;

namespace Corsinvest.ProxmoxVE.Admin.Module.MetricsExporter;

public class Module : ModuleBase
{
    internal static ConcurrentDictionary<string, Info> Infos { get; } = new();
    private static readonly Lock _infosLock = new();
    private static string PrometheusExporterUrl { get; set; } = string.Empty;

    public Module()
    {
        Keywords = "metrics,prometheus,exporter,monitoring,performance,statistics,observability";
        ModuleType = ModuleType.Application;
        Name = "Metrics Exporter";
        Description = "Exposes Proxmox VE metrics for monitoring systems";
        Category = Categories.Health;
        Slug = "metrics-exporter";
        HelpUrl = "modules/metrics-exporter";

        if (string.IsNullOrEmpty(PrometheusExporterUrl))
        {
            PrometheusExporterUrl = $"{GetBaseUrl(ApplicationHelper.AllClusterName)}/prometheus";
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

    internal static string GetUrl(string clusterName) => $"{PrometheusExporterUrl}/{clusterName}";

    protected override Task RefreshSettingsAsync(IServiceScope scope)
    {
        // Drop cached engines/registries so the next scrape rebuilds them with the new settings.
        Infos.Clear();
        return Task.CompletedTask;
    }

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        => AddSettings<Settings, Components.RenderSettings>(services);

    protected override void Map(WebApplication app) => MapExporterPrometheusMetrics(app);

    private static Info CreateInfo(string clusterName,
                                   Settings settings,
                                   IServiceScopeFactory scopeFactory,
                                   ILogger<Module> logger,
                                   ILogger<PrometheusMetricsEngine> engineLogger)
    {
        var registry = Prometheus.Metrics.NewCustomRegistry();
        var engine = new PrometheusMetricsEngine(settings.ApiSettings.Prometheus, registry, engineLogger);

        registry.AddBeforeCollectCallback(async () =>
        {
            using var scope = scopeFactory.CreateScope();
            try
            {
                var client = await scope.GetAdminService()[clusterName].GetPveClientAsync();
                await engine.CollectAsync(client);
            }
            catch (Exception ex) { logger.LogError(ex, ex.Message); }
        });

        return new() { Registry = registry };
    }

    private static void MapExporterPrometheusMetrics(WebApplication app)
        => app.MapGet(PrometheusExporterUrl + "/{clusterName}", async (string clusterName,
                                                             HttpContext context,
                                                             ILogger<Module> logger,
                                                             ILogger<PrometheusMetricsEngine> engineLogger,
                                                             IServiceScopeFactory scopeFactory) =>
        {
            using var outerScope = scopeFactory.CreateScope();
            var settingsService = outerScope.GetSettingsService();
            if (settingsService.GetEnabledClustersSettings().Any(a => a.Name == clusterName))
            {
                var settings = settingsService.GetForModule<Module, Settings>(clusterName);
                if (!settings.Enabled || !settings.ApiSettings.Prometheus.Enabled)
                {
                    return Results.Problem("Metrics Exporter is disabled", statusCode: StatusCodes.Status503ServiceUnavailable);
                }

                if (string.IsNullOrWhiteSpace(settings.Token))
                {
                    return Results.BadRequest("Token in setting not configured!");
                }

                var token = context.Request.Query["token"].ToString();
                var tokenBytes = Encoding.UTF8.GetBytes(token);
                var expectedBytes = Encoding.UTF8.GetBytes(settings.Token);
                if (!CryptographicOperations.FixedTimeEquals(tokenBytes, expectedBytes)) { return Results.Unauthorized(); }

                Info? info;
                // Two scrapes arriving together must not build two registries for the same cluster
                lock (_infosLock)
                {
                    if (!Infos.TryGetValue(clusterName, out info))
                    {
                        info = CreateInfo(clusterName, settings, scopeFactory, logger, engineLogger);
                        Infos[clusterName] = info;
                    }
                }

                //update statistic
                info.RegisterRequest();

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
