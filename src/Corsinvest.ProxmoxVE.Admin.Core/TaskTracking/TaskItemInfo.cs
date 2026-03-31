/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;

public class TaskItemInfo
{
    public int Id { get; init; }
    public string Title { get; init; } = default!;
    public string ClusterName { get; init; } = default!;
    public string? ModuleName { get; init; }
    public string? DetailUrl { get; init; }
    public bool IsCancellable { get; init; }
    public string CreatedBy { get; init; } = default!;
    public DateTime StartedAt { get; init; }

    public TaskItemStatus Status { get; set; }
    public string? Phase { get; set; }
    public int? Progress { get; set; }
    public string? LastLog { get; set; }
    public DateTime? EndedAt { get; set; }

    public TimeSpan Duration
        => EndedAt.HasValue
            ? (EndedAt - StartedAt).Value
            : TimeSpan.Zero;
}
