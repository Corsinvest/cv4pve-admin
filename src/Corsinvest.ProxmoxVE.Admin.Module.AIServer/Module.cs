/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Diagnostics;
using System.Text;
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Corsinvest.ProxmoxVE.Admin.Module.AIServer;

public class Module : ModuleBase
{
    public Module()
    {
        Keywords = "mcp,protocol,ai,integration,api,claude,assistant,tools,llm,context";
        ModuleType = ModuleType.Application;
        Name = "AI Server";
        Description = "Model Context Protocol server for AI integration with Proxmox VE";
        Category = Categories.Utilities;
        Scope = ClusterScope.Single;
        Slug = "ai-server";

        NavBar = [
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
            Icon = "hub",
            Render = NavBar.ToList()[0].Render
        };
    }

    protected override string PermissionBaseKey => "AiServer";

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        AddSettings<Settings, Components.RenderSettings>(services);

        services.AddMcpServer()
                .WithHttpTransport()
                .WithTools(GetTools());
    }

    protected virtual IEnumerable<Type> GetTools() => [typeof(Tools.VmTools)];

    protected override void Map(WebApplication app) =>
        app.MapMcp("/mcp/{clusterName}")
           .AddEndpointFilter(async (context, next) =>
           {
               var httpContext = context.HttpContext;
               using var scope = httpContext.RequestServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
               var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
               var logger = httpContext.RequestServices.GetRequiredService<ILogger<Module>>();

               // Get cluster name from route values
               var clusterNameFromRoute = httpContext.GetRouteValue("clusterName")?.ToString();
               if (string.IsNullOrWhiteSpace(clusterNameFromRoute))
               {
                   return Results.Json(new { error = "Cluster name required" }, statusCode: 400);
               }

               // Find the real cluster name with correct capitalization
               var clusterName = settingsService.GetEnabledClustersSettings()
                                                .FirstOrDefault(c => c.Name.Equals(clusterNameFromRoute, StringComparison.OrdinalIgnoreCase))?.Name;

               if (string.IsNullOrWhiteSpace(clusterName))
               {
                   return Results.Json(new { error = $"Cluster '{clusterNameFromRoute}' not found or disabled" }, statusCode: 400);
               }

               // Get settings for the specific cluster
               var settings = settingsService.GetForModule<Module, Settings>(clusterName);
               if (!settings.Enabled) { return Results.Json(new { error = "MCP Server is disabled for this cluster" }, statusCode: 503); }

               if (!httpContext.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
               {
                   return Results.Json(new { error = "API Key required" }, statusCode: 401);
               }

               if (!string.Equals(apiKey, settings.ApiToken, StringComparison.Ordinal))
               {
                   return Results.Json(new { error = "Invalid API Key" }, statusCode: 401);
               }

               httpContext.Items["ClusterName"] = clusterName;
               logger.LogInformation("MCP request for cluster {Cluster}", clusterName);

               httpContext.Request.EnableBuffering();

               using var reader = new StreamReader(
                   httpContext.Request.Body,
                   Encoding.UTF8,
                   detectEncodingFromByteOrderMarks: false,
                   leaveOpen: true
               );

               string body = await reader.ReadToEndAsync();

               // Riporta lo stream all'inizio (fondamentale)
               httpContext.Request.Body.Position = 0;

               // Stampa / log
               Debug.WriteLine("=============================");
               Debug.WriteLine(body);

               return await next(context);
           });
}
