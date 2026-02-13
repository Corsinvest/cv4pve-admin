/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class PveAdminUIHelper
{
    public static string GetColorRange(double value)
        => value > 80
            ? Colors.Danger
            : value > 70
                ? Colors.Warning
                : Colors.Success;

    public static ProgressBarStyle ToProgressBarStyle(string color)
        => color switch
        {
            Colors.Danger => ProgressBarStyle.Danger,
            Colors.Info => ProgressBarStyle.Info,
            Colors.Primary => ProgressBarStyle.Primary,
            Colors.Secondary => ProgressBarStyle.Secondary,
            Colors.Success => ProgressBarStyle.Success,
            Colors.Warning => ProgressBarStyle.Warning,
            _ => ProgressBarStyle.Primary
        };

    public static BadgeStyle ToBadgeStyle(string color)
        => color switch
        {
            Colors.Danger => BadgeStyle.Danger,
            Colors.Info => BadgeStyle.Info,
            Colors.Primary => BadgeStyle.Primary,
            Colors.Secondary => BadgeStyle.Secondary,
            Colors.Success => BadgeStyle.Success,
            Colors.Warning => BadgeStyle.Warning,
            _ => BadgeStyle.Primary
        };

    public static class CephCluster
    {
        public static string GetIconStatus(string status)
            => status switch
            {
                "HEALTH_OK" => "check_circle",
                "HEALTH_UPGRADE" => "upgrade",
                "HEALTH_OLD" => "warning",
                "HEALTH_WARN" => "warning",
                "HEALTH_ERR" => "error",
                _ => "error"
            };

        public static string GetColorStatus(string status)
            => status switch
            {
                "HEALTH_OK" => Colors.Success,
                "HEALTH_UPGRADE" => Colors.Warning,
                "HEALTH_OLD" => Colors.Warning,
                "HEALTH_WARN" => Colors.Warning,
                "HEALTH_ERR" => Colors.Danger,
                _ => Colors.Danger
            };
    }

    public static class Icons
    {
        public static string Cpu { get; } = "developer_board";
        public static string Memory { get; } = "memory";
        public static string Storage { get; } = "storage";
        public static string Network { get; } = "lan";
        public static string Node { get; } = "domain";
        public static string Vm { get; } = "desktop_windows";
        public static string Snapshot { get; } = "photo_camera";
        public static string Replication { get; } = "sync";
        public static string Backup { get; } = "backup";
        public static string Cluster { get; } = "dns";
        public static string Pool { get; } = "sell";

        // Common NavBar Icons
        public static string Overview { get; } = "home";
        public static string Dashboard { get; } = "dashboard";
        public static string Results { get; } = "check_circle";
        public static string Scans { get; } = "history";
        public static string Status { get; } = "info";
        public static string Trends { get; } = "trending_up";
        public static string Schedule { get; } = "schedule";
        public static string Jobs { get; } = "work";
        public static string Timeline { get; } = "timeline";
        public static string Errors { get; } = "error";
        public static string Warning { get; } = "warning";

        public static string GetWebConsoleType(WebConsoleType type)
            => type switch
            {
                WebConsoleType.Spice => "desktop_windows",
                WebConsoleType.NoVnc => "web_asset",
                WebConsoleType.XtermJs => "terminal",
                _ => "web_asset"
            };

        public static string GetResourceType(string type)
           => type switch
           {
               var s when s == PveConstants.KeyApiCluster => Cluster,
               var s when s == PveConstants.KeyApiNode => Node,
               var s when s == PveConstants.KeyApiPool => Pool,
               var s when s == PveConstants.KeyApiStorage => Storage,
               var s when s == PveConstants.KeyApiQemu => "desktop_windows",
               var s when s == PveConstants.KeyApiLxc => "terminal",
               var s when s == PveConstants.KeyApiTemplate => "description",
               var s when s == "sdn" => "transition_dissolve",
               _ => string.Empty
           };

        public static string GetVmType(VmType type) => GetResourceType(type.ToString().ToLower());

        public static string GetResourceStatus(string status, bool locked, bool allIcons)
            => locked
                ? "lock"
                : status switch
                {
                    var s when s == PveConstants.StatusVmRunning => "play_arrow",
                    var s when s == PveConstants.StatusVmStopped => allIcons ? "stop" : string.Empty,
                    var s when s == PveConstants.StatusVmPaused => "pause",
                    var s when s == PveConstants.StatusUnknown => "question_mark",
                    var s when s == PveConstants.StatusOnline => "check",
                    var s when s == PveConstants.StatusAvailable => "check",
                    //var s when s == "ok" => "check",
                    _ => string.Empty
                };
    }

    public static string GetResourcesColorStatus(string status, bool locked)
        => locked
            ? Colors.Warning
            : status switch
            {
                var s when s == PveConstants.StatusVmRunning => Colors.Success,
                var s when s == PveConstants.StatusVmStopped => Colors.Danger,
                var s when s == PveConstants.StatusVmPaused => Colors.Warning,
                var s when s == PveConstants.StatusUnknown => Colors.Warning,
                var s when s == PveConstants.StatusOnline => Colors.Success,
                var s when s == PveConstants.StatusAvailable => Colors.Success,
                var s when s == PveConstants.StatusOffline => Colors.Danger,
                _ => Colors.Primary
            };

    public static async Task<bool> PopulateClusterSettingsAsync(IAdminService adminService,
                                                                ClusterSettings clusterSettings,
                                                                DialogService dialogService,
                                                                NotificationService notificationService,
                                                                IStringLocalizer stringLocalizer)
    {
        var valid = false;

        _ = Task.Run(async () =>
        {
            await Task.Delay(1000);

            NotificationMessage message;

            try
            {
                var ret = await adminService.PopulateInfoAsync(clusterSettings);

                message = ret.IsSuccess
                            ? new()
                            {
                                Severity = NotificationSeverity.Success,
                                Summary = stringLocalizer["Info"],
                                Detail = stringLocalizer[ret.Value]
                            }
                            : new()
                            {
                                Severity = NotificationSeverity.Error,
                                Summary = stringLocalizer["Error"],
                                Detail = stringLocalizer[ret.Errors.Select(a => a.Message).JoinAsString(",")],
                                Duration = 20000
                            };

                valid = ret.IsSuccess;
            }
            catch (Exception ex)
            {
                message = new()
                {
                    Severity = NotificationSeverity.Error,
                    Summary = stringLocalizer["Exception"],
                    Detail = ex.Message,
                    Duration = 20000
                };
            }

            dialogService.Close();

            notificationService.Notify(message);
        });

        await dialogService.BusyAsync(stringLocalizer["Connecting to Proxmox VE"]);

        return valid;
    }

    public static IEnumerable<string> GetTagsStyle(string tags, string tagStyleColorMap)
    {
        var ret = new List<string>();
        var styles = (tagStyleColorMap ?? string.Empty).Split(";");

        foreach (var tag in Extensions.ClusterResourceExtensions.SplitTags(tags))
        {
            var found = false;
            foreach (var item in styles)
            {
                var def = item.Split(":");
                if (def[0] == tag)
                {
                    var color = def.Length >= 3
                                    ? $"color: #{def[2]};"
                                    : string.Empty;

                    var backgroundColor = def.Length >= 2
                                            ? $"background-color: #{def[1]};"
                                            : string.Empty;

                    found = true;
                    ret.Add($"<span class='rz-badge' style='{color}{backgroundColor}'>{tag}</span>");
                    break;
                }
            }

            if (!found)
            {
                ret.Add($"<span class='rz-badge rz-badge-danger rz-variant-filled rz-shade-lighter'>{tag}</span>");
            }
        }

        return ret;
    }
}
