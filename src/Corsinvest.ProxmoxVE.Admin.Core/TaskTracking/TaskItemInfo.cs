/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;

public record TaskItemInfo(int Id,
                           string Title,
                           string ClusterName,
                           string? ModuleName,
                           string? DetailUrl,
                           bool IsCancellable,
                           TaskItemStatus Status,
                           string? Phase,
                           DateTime StartedAt,
                           DateTime? EndedAt,
                           string CreatedBy,
                           int? Progress,
                           string? LastLog)
{
    public TimeSpan Duration
        => EndedAt.HasValue
            ? (EndedAt - StartedAt).Value
            : TimeSpan.Zero;
}
