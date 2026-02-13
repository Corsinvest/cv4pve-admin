/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Extensions;
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Core.Services;

namespace Corsinvest.ProxmoxVE.Admin.Module.MetricsExporter.Components;

public partial class Status(ISettingsService settingsService,
                            IModuleService moduleService) : IDisposable
{
    private string PrometheusToken { get; set; } = default!;
    public bool PrometheusEnabled { get; private set; }

    private record Data(string FullName, string Url, DateTime? LastRequest, long CountRequest);

    private IEnumerable<Data> Items
    {
        get
        {
            foreach (var item in settingsService.GetEnabledClustersSettings())
            {
                DateTime? lastRequest = null;
                var countRequest = 0L;

                if (Module.Infos.TryGetValue(item.Name, out var info))
                {
                    lastRequest = info.LastRequest;
                    countRequest = info.CountRequest;
                }

                yield return new Data(item.FullName,
                                      Module.GetUrl(item.Name) + $"?token={PrometheusToken}",
                                      lastRequest,
                                      countRequest);
            }
        }
    }

    protected override void OnInitialized()
    {
        moduleService.Get<Module>()!.SettingsUpdated += SettingsUpdated;
        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = settingsService.GetForModule<Module, Settings>(ApplicationHelper.AllClusterName);
        PrometheusToken = settings.Prometheus.Token;
        PrometheusEnabled = settings.Prometheus.Enabled;
        StateHasChanged();
    }

    private void SettingsUpdated(object? sender, EventArgs e) => LoadSettings();
    public void Dispose() => moduleService.Get<Module>()!.SettingsUpdated -= SettingsUpdated;
}
