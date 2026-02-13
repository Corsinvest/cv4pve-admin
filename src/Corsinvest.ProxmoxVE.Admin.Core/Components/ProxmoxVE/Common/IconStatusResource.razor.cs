/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Common;

public partial class IconStatusResource
{
    [Parameter] public string Type { get; set; } = default!;
    [Parameter] public string Status { get; set; } = default!;
    [Parameter] public bool Locked { get; set; }

    private string StyleTypeStatus
        => Status == PveConstants.StatusVmStopped || Status == PveConstants.StatusUnknown
            ? "color: #888;"
            : string.Empty;
}
