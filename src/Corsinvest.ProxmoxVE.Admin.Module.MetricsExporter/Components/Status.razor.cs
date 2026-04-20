/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Extensions;
using Corsinvest.ProxmoxVE.Admin.Core.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Core.Services;
using Microsoft.AspNetCore.Components;

namespace Corsinvest.ProxmoxVE.Admin.Module.MetricsExporter.Components;

public partial class Status(ISettingsService settingsService,
                            IModuleService moduleService) : IClusterName, IDisposable
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private string PrometheusToken { get; set; } = default!;
    private bool Enabled { get; set; }
    private bool PrometheusEnabled { get; set; }
    private DateTime? LastRequest { get; set; }
    private long CountRequest { get; set; }

    private string Url => Module.GetUrl(ClusterName) + $"?token={PrometheusToken}";

    protected override void OnInitialized()
    {
        moduleService.Get<Module>()!.SettingsUpdated += SettingsUpdated;
        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = settingsService.GetForModule<Module, Settings>(ClusterName);
        PrometheusToken = settings.Token;
        Enabled = settings.Enabled;
        PrometheusEnabled = settings.ApiSettings?.Prometheus?.Enabled ?? false;

        if (Module.Infos.TryGetValue(ClusterName, out var info))
        {
            LastRequest = info.LastRequest;
            CountRequest = info.CountRequest;
        }
        else
        {
            LastRequest = null;
            CountRequest = 0;
        }

        StateHasChanged();
    }

    private void SettingsUpdated(object? sender, EventArgs e) => LoadSettings();
    public void Dispose() => moduleService.Get<Module>()!.SettingsUpdated -= SettingsUpdated;
}
