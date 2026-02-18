/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json;
using System.Text.Json.Serialization;
using Corsinvest.ProxmoxVE.Admin;
using Corsinvest.ProxmoxVE.Admin.Components;
using Corsinvest.ProxmoxVE.Admin.Core;
using Corsinvest.ProxmoxVE.Admin.Core.Cryptography.Json;
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var (_, extraSettingsWarning) = builder.Configuration.AddJsonFileSafe(Path.Combine(AppContext.BaseDirectory, "config", "appsettings.extra.json"));

// Add service defaults & Aspire client integrations.
builder.AddServiceAspireDefaults();

builder.Host.UseSerilog((_, config) => config.ReadFrom.Configuration(builder.Configuration));

Log.Logger.Information("Start cv4pve-admin....");
Log.Logger.Information("Version: {Version}", BuildInfo.Version);
if (extraSettingsWarning != null) { Log.Logger.Warning(extraSettingsWarning); }

// Add services to the container.
builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

builder.Services.AddRadzenComponents();
builder.Services.AddRadzenCookieThemeService(options =>
{
    options.Name = ApplicationHelper.CookieThemeName;
    options.Duration = TimeSpan.FromDays(365);
});

builder.Services.AddLocalization();

var moduleTypes = new[]
{
    //system
    typeof(Corsinvest.ProxmoxVE.Admin.Module.System.Module),
    typeof(Corsinvest.ProxmoxVE.Admin.Module.Profile.Module),
    typeof(Corsinvest.ProxmoxVE.Admin.Module.Notifier.Smtp.Module),
    typeof(Corsinvest.ProxmoxVE.Admin.Module.Notifier.WebHook.Module),

    //application
    typeof(Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Module),
    typeof(Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Module),
    typeof(Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Module),
    typeof(Corsinvest.ProxmoxVE.Admin.Module.BackupAnalytics.Module),
    typeof(Corsinvest.ProxmoxVE.Admin.Module.ReplicationAnalytics.Module),
    typeof(Corsinvest.ProxmoxVE.Admin.Module.Dashboard.Module),
    typeof(Corsinvest.ProxmoxVE.Admin.Module.Resources.Module),
    typeof(Corsinvest.ProxmoxVE.Admin.Module.AIServer.Module),
    typeof(Corsinvest.ProxmoxVE.Admin.Module.Bots.Module),
    typeof(Corsinvest.ProxmoxVE.Admin.Module.MetricsExporter.Module),
    typeof(Corsinvest.ProxmoxVE.Admin.Module.Updater.Module),
    typeof(Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Module),
};

builder.Services.AddAdminCore(builder.Configuration, moduleTypes);

builder.Services.AddSingleton(sp =>
{
    return new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
        TypeInfoResolver = new EncryptAttributeJsonTypeInfoResolver(sp)
    };
});

var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var addresses = app.Services.GetRequiredService<IServer>()
                                .Features
                                .Get<IServerAddressesFeature>()?.Addresses ?? [];

    foreach (var address in addresses)
    {
        app.Logger.LogInformation("Listening on: {address}", address);
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseWebSockets();

app.UseHttpsRedirection();
//app.MapControllers();
app.MapStaticAssets();
app.UseAntiforgery();

app.UseAdminCore();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode()
   .AddAdditionalAssemblies([.. app.Services.GetRequiredService<IModuleService>().Assemblies]);

app.UseStatusCodePagesWithRedirects("/NotFound");

await app.RunAdminCoreAsync(args);

app.Run();

