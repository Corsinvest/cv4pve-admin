/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;
using Corsinvest.ProxmoxVE.Admin.Module.AIServer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Corsinvest.ProxmoxVE.Admin.Module.AIServer;

public class Module : ModuleBase
{
    public const string AppTokenName = "AI Server";
    public const string RoleName = "AI Server Tools";
    private const string HeaderApiKey = "X-API-Key";

    public Module()
    {
        Keywords = "mcp,protocol,ai,integration,api,claude,assistant,tools,llm,context";
        ModuleType = ModuleType.Application;
        Name = "AI Server";
        Description = "Model Context Protocol server for AI integration with Proxmox VE";
        Category = Categories.Utilities;
        Scope = ClusterScope.All;
        Slug = "ai-server";
        HelpUrl = "modules/ai-server";

        NavBar = [
            new(this,"Overview",string.Empty)
            {
                Render = new(typeof(Components.Overview)),
                Icon = PveAdminUIHelper.Icons.Overview
            },
            new(this,"API Access")
            {
                Render = new(typeof(Components.ApiAccess)),
                Icon = PveAdminUIHelper.Icons.Status
            },
            new(this,"Bridge")
            {
                Render = new(typeof(Components.Bridge)),
                Icon = "cable"
            }
        ];

        Link = new(this, Name, string.Empty)
        {
            Icon = "hub",
            Render = NavBar.ToList()[0].Render
        };

        Roles =
        [
            new(RoleName,
                "AI Server Tools",
                false,
                true,
                [// VM tools
                 Permissions.Tools.ListVms,
                 Permissions.Tools.ListSnapshots,
                 Permissions.Tools.GetVmConfig,
                 Permissions.Tools.ChangeVmState,
                 Permissions.Tools.CreateVmSnapshot,
                 Permissions.Tools.DeleteVmSnapshot,
                 Permissions.Tools.RollbackVmSnapshot,
                 Permissions.Tools.MigrateVm,
                 Permissions.Tools.BackupVm,
                 Permissions.Tools.ListVmRrdData,
                 // Node tools
                 Permissions.Tools.ListNodes,
                 Permissions.Tools.GetNodeStatus,
                 Permissions.Tools.ListReplications,
                 Permissions.Tools.ListNodeRrdData,
                 Permissions.Tools.ListTasks,
                 // Storage tools
                 Permissions.Tools.ListStorage,
                 Permissions.Tools.ListPools,
                 Permissions.Tools.ListBackups,
                 Permissions.Tools.ListStorageContent,
                 Permissions.Tools.ListBackupJobs,
                 Permissions.Tools.DeleteBackup,
                 Permissions.Tools.DeleteStorageContent,
                 Permissions.Tools.ListIsos,
                 Permissions.Tools.ListTemplates,
                 // Cluster tools
                 Permissions.Tools.GetClusterStatus,
                 Permissions.Tools.GetClusterOptions])
        ];
    }

    protected override string PermissionBaseKey => Permissions.BaseName;

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        AddSettings<Settings, Components.RenderSettings>(services);
        services.AddScoped<IAiServerService, AiServerService>();

        services.AddMcpServer()
                .WithHttpTransport()
                .WithTools(GetTools());
    }

    public override Task FixAsync(IServiceScope scope) => RunAsync(scope);

    protected override async Task RunAsync(IServiceScope scope)
    {
        // Create default app token with role
        var appTokenService = scope.GetRequiredService<IAppTokenService>();
        var token = await appTokenService.GetByName(AppTokenName);
        if (token is null)
        {
            (_, token) = await appTokenService.GenerateAsync(AppTokenName, null, null);
        }

        await appTokenService.SyncRolesAsync(token.Id, [RoleName, ClusterPermissions.RoleAdmin.Key]);
    }

    protected virtual IEnumerable<Type> GetTools()
        => [typeof(Tools.ClusterTools),
            typeof(Tools.NodeTools),
            typeof(Tools.StorageTools),
            typeof(Tools.VmTools)];

    protected override void Map(WebApplication app) =>
        app.MapMcp("/mcp")
           .AddEndpointFilter(async (context, next) =>
           {
               var httpContext = context.HttpContext;
               using var scope = httpContext.RequestServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
               var settingsService = scope.GetRequiredService<ISettingsService>();
               var logger = httpContext.RequestServices.GetRequiredService<ILogger<Module>>();

               // Get settings
               var settings = settingsService.GetForModule<Module, Settings>(ApplicationHelper.AllClusterName);
               if (!settings.Enabled)
               {
                   httpContext.Response.StatusCode = 503;
                   await httpContext.Response.WriteAsJsonAsync(new { error = "MCP Server is disabled" });
                   return Results.Empty;
               }

               if (!httpContext.Request.Headers.TryGetValue(HeaderApiKey, out var apiKey))
               {
                   httpContext.Response.StatusCode = 401;
                   await httpContext.Response.WriteAsJsonAsync(new { error = $"'{HeaderApiKey}' API Key required" });
                   return Results.Empty;
               }

               var auditService = scope.GetRequiredService<IAuditService>();
               var appTokenService = scope.GetRequiredService<IAppTokenService>();
               var validation = await appTokenService.ValidateAsync(apiKey!);
               if (!validation.IsValid)
               {
                   var tokenFingerprint = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(apiKey!)))[..12];
                   await auditService.LogAsync("MCP.Auth",
                                               false,
                                               $"{validation.Describe()} (fingerprint: {tokenFingerprint})");

                   await scope.GetEventNotificationService().PublishAsync(new McpAuthFailedNotification());
                   httpContext.Response.StatusCode = 401;
                   await httpContext.Response.WriteAsJsonAsync(new { error = "Invalid or expired token" });
                   return Results.Empty;
               }

               var token = validation.Token!;
               httpContext.User = new ClaimsPrincipal(new ClaimsIdentity([
                   new Claim(ApplicationClaimTypes.AppTokenId, token.Id.ToString())
               ], "AppToken"));

               httpContext.Request.EnableBuffering();
               using var reader = new StreamReader(httpContext.Request.Body,
                                                   Encoding.UTF8,
                                                   detectEncodingFromByteOrderMarks: false,
                                                   leaveOpen: true);
               var body = await reader.ReadToEndAsync();
               httpContext.Request.Body.Position = 0;

#if DEBUG
               Debug.WriteLine("=============================");
               Debug.WriteLine(body);
#endif

               var auditDetails = BuildMcpAuditDetails(body);
               await auditService.LogAsync("MCP.Request",
                                           true,
                                           $"Token: {token.Name}\n" +
                                           $"{httpContext.Request.Method} {httpContext.Request.Path}\n" +
                                           auditDetails);

               return await next(context);
           });

    private static string BuildMcpAuditDetails(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) { return "empty body"; }

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var method = root.TryGetProperty("method", out var m) ? m.GetString() ?? "?" : "?";
            var id = root.TryGetProperty("id", out var idEl) ? idEl.ToString() : "?";

            if (root.TryGetProperty("params", out var paramsEl) && paramsEl.ValueKind == JsonValueKind.Object)
            {
                var toolName = paramsEl.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null;

                if (paramsEl.TryGetProperty("arguments", out var argsEl) && argsEl.ValueKind == JsonValueKind.Object)
                {
                    var pairs = argsEl.EnumerateObject()
                                      .Select(p => KeyValuePair.Create<string, string?>(p.Name, p.Value.ToString()));
                    var argsFormatted = SensitiveDataHelper.FormatPairs(pairs);

                    return toolName != null
                        ? $"method={method} id={id} tool={toolName} args=[{argsFormatted}]"
                        : $"method={method} id={id} args=[{argsFormatted}]";
                }

                return toolName != null
                        ? $"method={method} id={id} tool={toolName}"
                        : $"method={method} id={id}";
            }

            return $"method={method} id={id}";
        }
        catch (JsonException)
        {
            return "invalid JSON body";
        }
    }
}
