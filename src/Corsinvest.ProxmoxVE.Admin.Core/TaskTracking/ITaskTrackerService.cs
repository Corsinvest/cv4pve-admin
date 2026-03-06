/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Linq.Expressions;
using System.Security.Claims;

namespace Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;

public interface ITaskTrackerService
{
    event EventHandler<TaskItem>? Changed;
    event EventHandler? AcknowledgedChanged;

    Task<TaskScope> StartAsync(string title,
                               string clusterName,
                               string? moduleName = null,
                               string? referenceId = null,
                               string? detailUrl = null,
                               TimeSpan? timeoutAfter = null,
                               bool cancellable = false);

    Task CancelAsync(int id);
    Task AcknowledgeAsync(IEnumerable<int> ids);
    Task AbandonStaleTasksAsync();
    Task<int> CleanupAsync(int retentionDays);
    Task<IReadOnlyList<TResult>> GetRecentAsync<TResult>(ClaimsPrincipal principal, Expression<Func<TaskItem, TResult>> selector, int max = 15, Expression<Func<TaskItem, bool>>? filter = null);
    Task<IReadOnlyList<string>> GetLogsAsync(int id);
}
