/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core;
using Corsinvest.AppHero.Core.Extensions;
using Corsinvest.AppHero.Core.Helpers;
using Corsinvest.AppHero.Core.MudBlazorUI.Style;
using Corsinvest.AppHero.Core.SoftwareRelease;
using Corsinvest.ProxmoxVE.Admin;
using Corsinvest.ProxmoxVE.Admin.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Serilog;

//appsetting default
var appSetting = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
if (!File.Exists(appSetting) || new FileInfo(appSetting).Length == 0)
{
    File.WriteAllText(appSetting, File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Default.json")));
}

var builder = WebApplication.CreateBuilder(args);

//configure serilog
builder.Host.UseSerilog((_, config) => config.ReadFrom.Configuration(builder.Configuration));

var logger = builder.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Main");

logger.LogInformation("Start cv4pve-admin....");

// Add services to the container.
builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(ApplicationHelper.PathData, "data-protection-keys")));
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options =>
                {
                    options.DetailedErrors = true;
                    //options.DisconnectedCircuitMaxRetained = 100;
                    //options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
                    //options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
                    //options.MaxBufferedUnacknowledgedRenderBatches = 10;
                })
                .AddHubOptions(options =>
                {
                    //options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                    options.EnableDetailedErrors = false;
                    //options.HandshakeTimeout = TimeSpan.FromSeconds(15);
                    //options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                    //options.MaximumParallelInvocationsPerClient = 100;
                    //options.MaximumReceiveMessageSize = 64 * 1024;
                    //options.StreamBufferCapacity = 10;
                })
                .AddCircuitOptions(option => { option.DetailedErrors = true; });

ThemeOptions.DefaultTheme.Palette.Primary = "#8c33b5ff";
ThemeOptions.DefaultTheme.PaletteDark.Primary = ThemeOptions.DefaultTheme.Palette.Primary;

builder.Services.AddHealthChecks();

builder.Services.ConfigureApp();

builder.Services.AddMvc();
builder.Services.AddControllers();

builder.Services.AddAppHero(builder.Configuration)
                .Customize()
                .AddReleaseGitHub()
                .AddReleaseDockerHub();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapHealthChecks("/health");

//migration
app.DatabaseMigrate<ApplicationDbContext>();

await app.OnPreApplicationInitializationAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    #region Swagger
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", $"cv4pve-admin API V1");
    });
    #endregion
}

app.UseHttpsRedirection();

app.UseRouting();

await app.OnApplicationInitializationAsync();

app.MapBlazorHub();
app.MapFallbackToPage("/_Index");

await app.OnPostApplicationInitializationAsync();

app.Run();