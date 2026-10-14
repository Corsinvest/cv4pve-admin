/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.BackupAnalytics.Components.Widgets.Check;

public class CheckSettings
{
    // The same as the days of data kept by default (MaxDaysLogs): older runs are not there
    public int Day { get; set; } = 30;
}
