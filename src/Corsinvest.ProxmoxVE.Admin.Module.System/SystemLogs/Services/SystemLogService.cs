/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.System.SystemLogs.Services;

internal class SystemLogService : ISystemLogService
{
    public Task<int> CleanupAsync(int retentionDays) => Task.FromResult(0);
}
