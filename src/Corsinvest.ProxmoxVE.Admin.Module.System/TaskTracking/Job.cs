/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;
using Corsinvest.ProxmoxVE.Admin.Module.System.TaskTracking.Extensions;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.TaskTracking;

internal class Job(IServiceScopeFactory scopeFactory)
{
    public async Task TaskCleanupAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var appSettings = scope.GetSettingsService().GetAppSettings();
        await scope.GetRequiredService<ITaskTrackerService>().CleanupAsync(appSettings.TaskCleanupRetentionDays);
    }
}
