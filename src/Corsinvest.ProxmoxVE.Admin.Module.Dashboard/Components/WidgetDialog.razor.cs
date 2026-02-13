/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.Dashboard.Components;

public partial class WidgetDialog : IModelParameter<WidgetEdit>
{
    [Parameter] public WidgetEdit Model { get; set; } = default!;

    private Dictionary<string, object> RenderParameters { get; set; } = [];
    protected override void OnInitialized()
    {
        if (Model.Settings != null)
        {
            RenderParameters.Add(nameof(ISettingsParameter<>.Settings), Model.Settings);
        }
    }
}
