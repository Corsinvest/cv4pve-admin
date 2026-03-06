/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.System.TaskTracking.Extensions;

internal static class JobExtensions
{
    public static void InitializeTaskCleanupJob(this IServiceScope scope)
    {
        var backgroundJobService = scope.GetBackgroundJobService();
        var appSettings = scope.GetSettingsService().GetAppSettings();

        backgroundJobService.ScheduleOrRemove<Job>(a => a.TaskCleanupAsync(),
                                                   appSettings.TaskCleanupCronSchedule,
                                                   true,
                                                   "_");
    }
}
