/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.Dashboard.Components.Widgets.Memo;

public partial class Render : IModuleWidget<Settings>
{
    [Parameter] public Settings Settings { get; set; } = default!;
    [Parameter] public EventCallback<Settings> SettingsChanged { get; set; }
    [Parameter] public bool InEditing { get; set; }
    [Parameter] public IEnumerable<string> ClusterNames { get; set; } = [];

    public Task RefreshDataAsync() => Task.CompletedTask;

    private async Task OnContentChanged(string content)
    {
        Settings.Content = content;
        await SettingsChanged.InvokeAsync(Settings);
    }
}
