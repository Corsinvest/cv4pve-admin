/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using BlazorDownloadFile;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Corsinvest.ProxmoxVE.Admin.Core.Clients.Pve;
using Corsinvest.ProxmoxVE.Admin.Core.Hooks;
using Corsinvest.ProxmoxVE.Admin.Core.Commands;
using Corsinvest.ProxmoxVE.Admin.Core.Common.Middleware;
using Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE;
using Corsinvest.ProxmoxVE.Admin.Core.Fonts;
using Corsinvest.ProxmoxVE.Admin.Core.Notifier;
using Corsinvest.ProxmoxVE.Admin.Core.Search;
using Corsinvest.ProxmoxVE.Admin.Core.Search.Providers;
using Corsinvest.ProxmoxVE.Admin.Core.Session;
using Corsinvest.ProxmoxVE.Admin.Core.ToolBarUtilities;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using PdfSharp.Fonts;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace Corsinvest.ProxmoxVE.Admin.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAdminCore(this IServiceCollection services, IConfiguration configuration, IEnumerable<Type> moduleTypes)
    {
        services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(ApplicationHelper.DataPath, "data-protection-keys")))
                .SetApplicationName("cv4pve-admin");
        services.AddBlazorDownloadFile();
        services.AddBlazoredLocalStorage();
        services.AddBlazoredSessionStorage();
        services.AddHotKeys2();

        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddScoped<ISnapshotSizeService, SnapshotSizeService>();
        services.AddSingleton<ISessionHubService, SessionHubService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddTransient<INotifierService, NotifierService>();
        services.AddScoped<IDialogServiceEx, DialogServiceEx>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ICurrentClusterService, CurrentClusterService>();
        services.AddScoped<IBrowserService, BrowserService>();
        services.AddScoped<IReportService, ReportService>();

        // Search Providers
        services.AddScoped<ISearchProvider, PveSearchProvider>();
        services.AddScoped<ISearchProvider, ModuleSearchProvider>();
        services.AddScoped<ISearchProvider, SystemSearchProvider>();

        services.AddScoped<IToolBarUtility<IClusterResourceNode>, NodeMemoryCleanup>();
        services.AddScoped<IToolBarUtility<IClusterResourceVm>, VmUnlock>();

        services.AddCommands(typeof(ServiceCollectionExtensions).Assembly);

        services.AddModules(configuration, moduleTypes);

        services.AddFusionCache();
        services.AddEventNotifyer();

        GlobalFontSettings.FontResolver = new PdfSharpEmbeddedFontResolver();

        services.AddMemoryCache();
        services.AddSingleton<ApplicationStateService>();

        // Hook Executor
        services.AddHookExecutor();

        // PVE Client Factory and HttpClients
        services.AddSingleton<IPveClientFactory, PveClientFactory>();

        services.AddHttpClient("HttpStrict")
            .ConfigureHttpClient(client => client.DefaultRequestHeaders.Add("User-Agent", "cv4pve-admin"));

        services.AddHttpClient("HttpIgnoreCert")
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            })
            .ConfigureHttpClient(client => client.DefaultRequestHeaders.Add("User-Agent", "cv4pve-admin"));

        return services;
    }

    private static IServiceCollection AddEventNotifyer(this IServiceCollection services)
    {
        services.AddSingleton<EventNotificationService>();
        return services;
    }

    public static void UseAdminCore(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.MapPveEndpoints();

        foreach (var item in app.Services.GetRequiredService<IModuleService>().Modules)
        {
            item.Map(app);
        }

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(ApplicationHelper.ImagesPath),
            RequestPath = "/data-images"
        });
    }

    public static async Task RunAdminCoreAsync(this IHost host, string[] args)
    {
        var moduleService = host.Services.GetRequiredService<IModuleService>();

        using var scope = host.Services.CreateScope();
        foreach (var item in moduleService.Modules) { await item.InitializeAsync(scope); }
        foreach (var item in moduleService.Modules) { await item.RunAsync(scope); }

        var applicationStateService = host.Services.GetRequiredService<ApplicationStateService>();
        applicationStateService.IsStartupComplete = true;
        applicationStateService.IsReady = true;

        // Check for CLI command
        await host.TryExecuteCliCommandAsync(args);
    }

    private static IServiceCollection AddModules(this IServiceCollection services, IConfiguration configuration, IEnumerable<Type> moduleTypes)
    {
        var modules = new List<ModuleBase>();
        foreach (var type in moduleTypes)
        {
            //ActivatorUtilities.CreateInstance()
            var module = (ModuleBase)Activator.CreateInstance(type)!;
            module.ConfigureServices(services, configuration);
            modules.Add(module);
        }

        //foreach (var module in modules) { services.AddSingleton(module.GetType(), module); }

        foreach (var assembly in modules.Select(m => m.GetType().Assembly).Distinct())
        {
            EventNotificationService.RegisterHandlersFromAssembly(services, assembly);
        }

        services.AddSingleton<IModuleService>(new ModuleService(modules));

        return services;
    }

    private static async Task TryExecuteCliCommandAsync(this IHost host, string[] args)
    {
        if (args.Length == 0
            || (args[0].StartsWith("--")
                && args[0] != "--help"
                && args[0] != "--version")) { return; }

        var rootCommand = new System.CommandLine.RootCommand("cv4pve-admin - Corsinvest Proxmox VE Admin")
        {
            Security.UserCommands.CreateUserCommand(host.Services)
        };

        Environment.Exit(await rootCommand.Parse(args).InvokeAsync());
    }
}
