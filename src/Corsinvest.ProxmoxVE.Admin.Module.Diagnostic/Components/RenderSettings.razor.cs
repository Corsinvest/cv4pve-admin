/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Components;

public partial class RenderSettings : IModelParameter<Settings>
{
    [Parameter] public Settings Model { get; set; } = default!;

    private static string GetIconType(int index)
        => PveAdminUIHelper.Icons.GetResourceType(new[] { PveConstants.KeyApiNode, PveConstants.KeyApiQemu, PveConstants.KeyApiLxc }[index]);
}
