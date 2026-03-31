/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Linq.Expressions;

namespace Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;

public static class TaskItemFilters
{
    /// <summary>
    /// Matches tasks that are not completed or ended within the last hour.
    /// </summary>
    public static readonly Expression<Func<TaskItem, bool>> Active = t =>
        t.Status != TaskItemStatus.Completed &&
        (t.EndedAt == null || t.EndedAt >= DateTime.UtcNow.AddHours(-1));
}
