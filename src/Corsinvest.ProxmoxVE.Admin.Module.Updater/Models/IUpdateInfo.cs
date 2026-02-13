/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.Updater.Models;

internal interface IUpdateInfo
{
    UpdateInfoStatus UpdateScanStatus { get; set; }
    bool UpdateNormalAvailable { get; set; }
    bool UpdateSecurityAvailable { get; set; }
    bool UpdateRequireReboot { get; set; }
}
