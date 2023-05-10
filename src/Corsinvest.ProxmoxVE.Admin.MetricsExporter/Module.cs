/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Extensions;
using Corsinvest.AppHero.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Core.Services;
using Corsinvest.ProxmoxVE.Admin.MetricsExporter.Components;
using Corsinvest.ProxmoxVE.Metrics.Exporter.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Corsinvest.ProxmoxVE.Admin.MetricsExporter;

public class Module : PveAdminModuleBase, IForceLoadModule
{
    public static string Url { get; } = "/pvePrometheusMetrics";
    internal static IDictionary<string, Info> Infos { get; } = new Dictionary<string, Info>();

    public Module()
    {
        Authors = "Corsinvest Srl";
        Company = "Corsinvest Srl";
        Keywords = "Metric,Exporter,Prometheus";
        Description = "Metrics Exporter";
        InfoText = "Prometeus exporter metrics for your Proxmox VE cluster";
        SetCategory(ModuleCategory.Health);

        Link = new ModuleLink(this, Description)
        {
            Icon = Icons.Material.Outlined.MultilineChart,
            Render = typeof(RenderIndex)
        };

        Widgets = new[]
        {
            new ModuleWidget(this,"Status")
            {
                GroupName = Category,
                Render = typeof(RenderWidget),
                Class = "mud-grid-item mud-grid-item-xs-12 mud-grid-item-sm-6 mud-grid-item-md-4 mud-grid-item-lg-4"
            }
        };

        UrlHelp += "#chapter_module_metrics_exporter";
    }

    public static string GetUrl(string clusterName) => $"{Url}/{clusterName}";

    public override void ConfigureServices(IServiceCollection services, IConfiguration config) => AddOptions<Options, RenderOptions>(services, config);

    public override async Task OnApplicationInitializationAsync(IHost host)
    {
        await Task.CompletedTask;
        MapExporterPrometheusMetrics((WebApplication)host);
    }

    private static void MapExporterPrometheusMetrics(WebApplication app)
    {
        app.MapGet(Url + "/{clusterName?}", (string clusterName,
                                            IOptionsMonitor<Options> options,
                                            ILogger<Module> logger,
                                            IPveClientService pveClientService) =>
        {
            var clusters = pveClientService.GetClusters();
            if (clusters.Any(a => a.Name == clusterName))
            {
                if (!Infos.TryGetValue(clusterName, out var info))
                {
                    //create register
                    var registry = Prometheus.Metrics.NewCustomRegistry();
                    registry.AddBeforeCollectCallback(async () =>
                    {
                        var moduleClusterOptions = options.CurrentValue.Get(clusterName);
                        var exporter = new PrometheusExporter(registry,
                                                              moduleClusterOptions.PrometheusExporterPrefix,
                                                              moduleClusterOptions.PrometheusExporterNodeDiskInfo);

                        try
                        {
                            await exporter.Collect(await pveClientService.GetClient(clusterName));
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
                using var ms = new MemoryStream();
                info.Registry.CollectAndExportAsTextAsync(ms);

                ms.Position = 0;
                using var sr = new StreamReader(ms);
                return sr.ReadToEnd();
            }
            else
            {
                return clusters.Select(a => $"url {Url}/{a.Name} for {a.FullName}").JoinAsString(Environment.NewLine);
            }
        });
    }
}