/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Common;

public partial class IconStatusResource
{
    [Parameter] public string Type { get; set; } = default!;
    [Parameter] public string Status { get; set; } = default!;
    [Parameter] public bool Locked { get; set; }

    private string GetStyleTypeStatus()
        => Status switch
        {
            var s when s == PveConstants.StatusVmStopped || s == PveConstants.StatusUnknown => "color: #888;",
            _ => "",
        };

    private string GetIconType() => PveBlazorHelper.Icons.GetResourceType(Type);
    private string GetIconStatus() => PveBlazorHelper.Icons.GetResourceStatus(Status, Locked);
    private Color GetColorStatus() => PveBlazorHelper.GetResourcesColorStatus(Status, Locked);
}