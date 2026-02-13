/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Common;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components.Widgets.Status;

public partial class Render : IModuleWidget<Settings>
{
    [Parameter] public Settings Settings { get; set; } = default!;
    [Parameter] public EventCallback<Settings> SettingsChanged { get; set; }
    [Parameter] public IEnumerable<string> ClusterNames { get; set; } = default!;
    [Parameter] public bool InEditing { get; set; }

    private GridResourceStatus GridResourceStatusRef { get; set; } = default!;
    public async Task RefreshDataAsync()
    {
        if (GridResourceStatusRef != null) { await GridResourceStatusRef.RefreshDataAsync(); }
    }
}
