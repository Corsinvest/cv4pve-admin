/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Linq.Expressions;

namespace Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;

public static class TaskItemFilters
{
    public static readonly Expression<Func<TaskItem, bool>> Active = t =>
        t.Status == TaskItemStatus.Running
        || (t.EndedAt != null && t.EndedAt >= DateTime.UtcNow.AddMinutes(-10))
        || ((t.Status == TaskItemStatus.Failed || t.Status == TaskItemStatus.Abandoned) && !t.IsAcknowledged);
}
