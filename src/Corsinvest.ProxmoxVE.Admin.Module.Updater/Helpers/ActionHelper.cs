/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.ComponentModel;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Module.Updater.Models;
using Corsinvest.ProxmoxVE.Admin.Module.Updater.Services;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Mapster;
using Microsoft.Extensions.Localization;

namespace Corsinvest.ProxmoxVE.Admin.Module.Updater.Helpers;

internal class ActionHelper : BaseActionHelper<Module, Settings, DataChangedNotification>
{
    private const string CachedKey = "VmsUpdateInfo";

    public static string GetDefaultScript(ScriptType scriptType)
    {
        var resource = $"{typeof(Module).Namespace}.Scripts.";
        resource += scriptType switch
        {
            ScriptType.LinuxSearchUpdate => "LinuxSearchUpdate.sh",
            //ScriptType.LinuxExecuteUpdate => "LinuxExecuteUpdate.sh",
            ScriptType.WindowsSearchUpdate => "WindowsSearchUpdate.ps1",
            //ScriptType.WindowsExecuteUpdate => "WindowsExecuteUpdate.ps1",
            _ => throw new InvalidEnumArgumentException()
        };

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static async Task<IEnumerable<ClusterResourceUpdateScanInfo>> GetAsync(ClusterClient clusterClient)
        => (await clusterClient.CachedData.GetOrDefaultAsync(CachedKey, Enumerable.Empty<ClusterResourceUpdateScanInfo>()))!;

    public static async Task ScanAsync(IServiceScope scope, string clusterName, bool automatic)
    {
        var logger = scope.GetLoggerFactory().CreateLogger<ActionHelper>();
        var auditService = scope.GetAuditService();

        using (logger.LogTimeOperation(LogLevel.Information, true, "Collect update data for cluster '{clusterName}'", clusterName))
        {
            var clusterClient = scope.GetClusterClient(clusterName);
            var eventNotificationService = scope.GetEventNotificationService();
            var settings = GetModuleSettings(scope, clusterName);

            await ScanAsync(clusterClient, settings, eventNotificationService, logger);

            var items = await GetAsync(clusterClient);

            //send notification
            if (items.Any(a => a.UpdateRequireReboot || a.UpdateNormalAvailable || a.UpdateSecurityAvailable)
                && settings.NotifierConfigurations?.Any() is true)
            {
                var updaterService = scope.GetRequiredService<IUpdaterService>();
                var L = scope.GetRequiredService<IStringLocalizer<ActionHelper>>();
                var appSettings = scope.GetSettingsService().GetAppSettings();

                await using var ms = updaterService.GeneratePdf(clusterName, items);

                await scope.GetNotifierService().SendAsync(settings.NotifierConfigurations, new()
                {
                    Subject = L["{0} - Update VM/CT of cluster {1}", appSettings.AppName, clusterName],
                    Body = L["Update result of {0}", items.Min(a => a.UpdateScanTimestamp)!],
                    Attachments = [new(ms, "Update.pdf", MediaTypeNames.Application.Pdf)]
                });
            }

            var itemsList = items.ToList();
            var vmCount = itemsList.Count(a => a.VmType == VmType.Qemu);
            var ctCount = itemsList.Count(a => a.VmType == VmType.Lxc);
            var securityCount = itemsList.Count(a => a.UpdateSecurityAvailable);
            var normalCount = itemsList.Count(a => a.UpdateNormalAvailable && !a.UpdateSecurityAvailable);
            var rebootCount = itemsList.Count(a => a.UpdateRequireReboot);

            await auditService.LogAsync("Updater.Scan", true, $"Cluster: {clusterName}, VMs: {vmCount}, CTs: {ctCount}, Security: {securityCount}, Normal: {normalCount}, Reboot: {rebootCount}");

            await PublishDataChangedAsync(scope);
        }
    }

    private static async Task ScanAsync(ClusterClient clusterClient,
                                        Settings settings,
                                        EventNotificationService eventNotificationService,
                                        ILogger logger)
    {
        var scriptLinux = settings.ScriptLinuxSearchUpdate.Replace("\r", string.Empty)
                                                          .Replace("\t", string.Empty);

        var scriptWindows = settings.ScriptWindowsSearchUpdate.Replace("\r", string.Empty)
                                                              .Replace("\t", string.Empty);

        var items = (await clusterClient.CachedData.GetResourcesAsync(false))
                            .Where(a => a.ResourceType == ClusterResourceType.Vm && a.IsRunning)
                            .AsQueryable()
                            .ProjectToType<ClusterResourceUpdateScanInfo>()
                            .ToList();

        await clusterClient.CachedData.GetOrSetAsync(CachedKey, items, 8 * 60 * 60, true);

        await eventNotificationService.PublishAsync(new DataChangedNotification());

        var client = await clusterClient.GetPveClientAsync();

        foreach (var node in items.Select(a => a.Node).Distinct().Order())
        {
            foreach (var item in items.Where(a => a.Node == node && a.IsRunning).OrderBy(a => a.VmType).ThenBy(a => a.Name))
            {
                logger.LogInformation("Scan {node}/{id} ", item.Node, item.Id);

                var ret = await client.VmExecNativeAsync(item.Node, item.VmType, item.VmId, scriptLinux, scriptWindows);
                if (ret.IsSuccess)
                {
                    try
                    {
                        var info = JsonSerializer.Deserialize<Info>(ret.Value)!;
                        item.UpdateScanStatus = info.UpdateCheck
                                                ? UpdateInfoStatus.Ok
                                                : UpdateInfoStatus.InError;
                        item.UpdateNormalAvailable = info.UpdateNormalAvailable;
                        item.UpdateSecurityAvailable = info.UpdateSecurityAvailable;
                        item.UpdateRequireReboot = info.RequireReboot;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error parsing JSON result: {result}", ret.Value);
                        item.UpdateScanStatus = UpdateInfoStatus.InError;
                        item.Error = $"Error parsing JSON result: {ret.Value}";
                    }
                }
                else
                {
                    item.UpdateScanStatus = UpdateInfoStatus.InError;
                    item.Error = ret.Errors.Select(a => a.Message).JoinAsString(", ");
                }

                item.UpdateScanTimestamp = DateTime.Now;

                await eventNotificationService.PublishAsync(new DataChangedNotification());
            }
        }
    }

    private class Info
    {
        [JsonPropertyName("update_check")]
        public bool UpdateCheck { get; set; }

        [JsonPropertyName("update_normal_available")]
        public bool UpdateNormalAvailable { get; set; }

        [JsonPropertyName("update_security_available")]
        public bool UpdateSecurityAvailable { get; set; }

        [JsonPropertyName("require_reboot")]
        public bool RequireReboot { get; set; }
    }
}
