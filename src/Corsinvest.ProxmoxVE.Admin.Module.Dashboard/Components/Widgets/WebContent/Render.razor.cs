/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Web;

namespace Corsinvest.ProxmoxVE.Admin.Module.Dashboard.Components.Widgets.WebContent;

public partial class Render : IModuleWidget<Settings>
{
    [Parameter] public Settings Settings { get; set; } = default!;
    [Parameter] public EventCallback<Settings> SettingsChanged { get; set; }
    [Parameter] public bool InEditing { get; set; }
    [Parameter] public IEnumerable<string> ClusterNames { get; set; } = [];

    private string IframeUrl { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    public Task RefreshDataAsync()
    {
        if (Settings?.Type == ContentType.Iframe && !string.IsNullOrWhiteSpace(Settings?.Url))
        {
            if (Uri.TryCreate(Settings.Url, UriKind.Absolute, out var uri))
            {
                var query = HttpUtility.ParseQueryString(uri.Query);
                var paramName = "cachebust";
                var counter = 1;

                while (query[paramName] != null)
                {
                    paramName = $"cachebust{counter++}";
                }
                query[paramName] = Guid.NewGuid().ToString();

                IframeUrl = new UriBuilder(uri) { Query = query.ToString() }.ToString();
            }
            else
            {
                IframeUrl = Settings.Url;
            }
        }
        else
        {
            IframeUrl = string.Empty;
        }

        return Task.CompletedTask;
    }
}
