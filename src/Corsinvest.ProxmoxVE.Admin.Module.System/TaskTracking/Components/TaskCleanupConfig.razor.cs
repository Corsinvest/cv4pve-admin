/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Configuration;
using Corsinvest.ProxmoxVE.Admin.Module.System.TaskTracking.Extensions;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.TaskTracking.Components;

public partial class TaskCleanupConfig : ISettingsParameter<AppSettings>
{
    [Parameter] public AppSettings Settings { get; set; } = default!;
    [Parameter] public EventCallback<AppSettings> SettingsChanged { get; set; } = default!;

    private int RetentionDays
    {
        get => Settings.TaskCleanupRetentionDays;
        set
        {
            Settings.TaskCleanupRetentionDays = value;
            SettingsChanged.InvokeAsync(Settings);
        }
    }

    private string CronSchedule
    {
        get => Settings.TaskCleanupCronSchedule;
        set
        {
            Settings.TaskCleanupCronSchedule = value;
            SettingsChanged.InvokeAsync(Settings);
        }
    }
}
