/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;
using DiagnosticApiSettings = Corsinvest.ProxmoxVE.Diagnostic.Api.Settings;

namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Components;

public partial class RenderSettings(NotificationService notificationService) : IModelParameter<Settings>
{
    [Parameter] public Settings Model { get; set; } = default!;

    private void ApplyPreset(DiagnosticApiSettings preset, string presetName)
    {
        Model.ApiSettings = preset;
        notificationService.Info(L[$"Settings set to {presetName} mode"]);
        StateHasChanged();
    }

    private static string GetIconType(int index)
        => PveAdminUIHelper.Icons.GetResourceType(new[] { PveConstants.KeyApiNode, PveConstants.KeyApiQemu, PveConstants.KeyApiLxc }[index]);
}
