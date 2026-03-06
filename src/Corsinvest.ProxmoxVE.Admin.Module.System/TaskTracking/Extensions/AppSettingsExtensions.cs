/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Configuration;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.TaskTracking.Extensions;

public static class AppSettingsExtensions
{
    private const string KeyRetentionDays = "TaskCleanup.RetentionDays";
    private const string KeyCronSchedule = "TaskCleanup.CronSchedule";

    extension(AppSettings source)
    {
        public int TaskCleanupRetentionDays
        {
            get => source.ExtendedData.Get(KeyRetentionDays, 30);
            set => source.ExtendedData.Set(KeyRetentionDays, value);
        }

        public string TaskCleanupCronSchedule
        {
            get => source.ExtendedData.Get(KeyCronSchedule, "0 4 * * *");
            set => source.ExtendedData.Set(KeyCronSchedule, value);
        }
    }
}
