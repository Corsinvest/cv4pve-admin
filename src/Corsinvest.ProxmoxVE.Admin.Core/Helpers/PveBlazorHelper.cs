/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.MudBlazorUI.Shared.Components.DataGrid;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;
using System.Linq.Expressions;

namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class PveBlazorHelper
{
    public static string GetColorRangeToString(double value)
    {
        if (value > 90) { return "var(--mud-palette-error)"; }
        else if (value > 80) { return "var(--mud-palette-warning)"; }
        else { return "var(--mud-palette-success)"; }
    }

    public static Color GetColorRange(double value)
    {
        if (value > .8) { return Color.Error; }
        else if (value > .7) { return Color.Warning; }
        else { return Color.Success; }
    }

    public class CephCluster
    {
        public static string GetIconStatus(string status)
            => status switch
            {
                "HEALTH_OK" => MudBlazor.Icons.Material.Outlined.CheckCircle,
                "HEALTH_UPGRADE" => MudBlazor.Icons.Material.Outlined.Upgrade,
                "HEALTH_OLD" => MudBlazor.Icons.Material.Outlined.Warning,
                "HEALTH_WARN" => MudBlazor.Icons.Material.Outlined.Warning,
                "HEALTH_ERR" => MudBlazor.Icons.Material.Outlined.Error,
                _ => MudBlazor.Icons.Material.Outlined.Error,
            };

        public static Color GetColorStatus(string status)
            => status switch
            {
                "HEALTH_OK" => Color.Success,
                "HEALTH_UPGRADE" => Color.Warning,
                "HEALTH_OLD" => Color.Warning,
                "HEALTH_WARN" => Color.Warning,
                "HEALTH_ERR" => Color.Error,
                _ => Color.Error,
            };
    }

    public static Color GetColorStatus(string status)
        => status.ToLower() switch
        {
            var s when s == PveConstants.StatusOnline => Color.Success,
            var s when s == PveConstants.StatusOffline => Color.Warning,
            var s when s == PveConstants.StatusVmRunning => Color.Success,
            var s when s == PveConstants.StatusVmPaused => Color.Warning,
            var s when s == PveConstants.StatusVmStopped => Color.Error,
            var s when s == PveConstants.StatusUnknown => Color.Warning,
            var s when s == PveConstants.KeyApiTemplate => Color.Default,
            _ => Color.Default,
        };

    public static Color GetResourcesColorStatus(string status, bool locked)
        => locked
           ? Color.Warning
           : status switch
           {
               var s when s == PveConstants.StatusVmRunning => Color.Success,
               var s when s == PveConstants.StatusVmStopped => Color.Default,
               var s when s == PveConstants.StatusVmPaused => Color.Warning,
               var s when s == PveConstants.StatusUnknown => Color.Warning,
               var s when s == PveConstants.StatusOnline => Color.Success,
               _ => Color.Default,
           };


    public static class Icons
    {
        public static string Cpu { get; } = MudBlazor.Icons.Material.Outlined.DeveloperBoard;
        public static string Memory { get; } = MudBlazor.Icons.Material.Outlined.Memory;
        public static string Storage { get; } = MudBlazor.Icons.Material.Outlined.Storage;
        public static string Network { get; } = MudBlazor.Icons.Material.Outlined.Lan;
        public static string Node { get; } = MudBlazor.Icons.Material.Outlined.Domain;
        public static string Snapshot { get; } = MudBlazor.Icons.Material.Outlined.CameraAlt;
        public static string Replication { get; } = MudBlazor.Icons.Material.Outlined.Sync;
        public static string Backup { get; } = MudBlazor.Icons.Material.Outlined.Backup;

        public static string GetStatus(string status)
            => status.ToLower() switch
            {
                var s when s == PveConstants.StatusOnline => MudBlazor.Icons.Material.Filled.Check,
                var s when s == PveConstants.StatusUnknown => MudBlazor.Icons.Material.Filled.QuestionMark,
                var s when s == PveConstants.StatusOffline => MudBlazor.Icons.Material.Filled.StopCircle,
                var s when s == PveConstants.StatusVmRunning => MudBlazor.Icons.Material.Filled.PlayCircle,
                var s when s == PveConstants.StatusVmPaused => MudBlazor.Icons.Material.Filled.PauseCircle,
                var s when s == PveConstants.StatusVmStopped => MudBlazor.Icons.Material.Filled.StopCircle,
                var s when s == PveConstants.KeyApiTemplate => MudBlazor.Icons.Material.Filled.FileOpen,
                _ => MudBlazor.Icons.Material.Outlined.WarningAmber,
            };

        public static string GetResourceType(string type)
            => type switch
            {
                var s when s == PveConstants.KeyApiCluster => MudBlazor.Icons.Material.Outlined.Dns,
                var s when s == PveConstants.KeyApiNode => Node,
                var s when s == PveConstants.KeyApiPool => MudBlazor.Icons.Material.Outlined.Sell,
                var s when s == PveConstants.KeyApiStorage => Storage,
                var s when s == PveConstants.KeyApiQemu => MudBlazor.Icons.Material.Outlined.DesktopWindows,
                var s when s == PveConstants.KeyApiLxc => MudBlazor.Icons.Material.Outlined.Terminal,
                var s when s == PveConstants.KeyApiTemplate => MudBlazor.Icons.Material.Outlined.Description,
                _ => "",
            };

        public static string GetVmType(VmType type) => GetResourceType(type.ToString().ToLower());

        public static string GetResourceStatus(string status, bool locked)
              => locked
                 ? MudBlazor.Icons.Material.Filled.Lock
                 : status switch
                 {
                     var s when s == PveConstants.StatusVmRunning => MudBlazor.Icons.Material.Filled.PlayArrow,
                     var s when s == PveConstants.StatusVmStopped => "",
                     var s when s == PveConstants.StatusVmPaused => MudBlazor.Icons.Material.Filled.Pause,
                     var s when s == PveConstants.StatusUnknown => MudBlazor.Icons.Material.Filled.QuestionMark,
                     var s when s == PveConstants.StatusOnline => MudBlazor.Icons.Material.Filled.Check,
                     _ => "",
                 };
    }

    public class AHPropertyColumn
    {
        public static Type GetDynamicType<T>(string propertyName)
            => typeof(AHPropertyColumn<,>).MakeGenericType([typeof(T), typeof(T).GetProperty(propertyName)!.PropertyType]);

        public static Dictionary<string, object> GetDynamicParameters<T>(string propertyName)
        {
            var propertyInfo = typeof(T).GetProperty(propertyName)!;
            var parameter = Expression.Parameter(typeof(T), "i");
            var property = Expression.Property(parameter, propertyInfo.Name);
            var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), propertyInfo.PropertyType);

            return new()
            {
                ["T"] = typeof(T),
                ["TProperty"] = propertyInfo.PropertyType,
                [nameof(AHPropertyColumn<Object, object>.Property)] = Expression.Lambda(delegateType, property, parameter),
                [nameof(AHPropertyColumn<Object, object>.FormatProvider)] = typeof(PveFormatProvider)
            };
        }
    }
}