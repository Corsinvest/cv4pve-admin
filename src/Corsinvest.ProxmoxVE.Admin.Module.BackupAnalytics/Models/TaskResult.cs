/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.BackupAnalytics.Models;

public class TaskResult : IClusterName, IId
{
    public int Id { get; set; }
    [Required] public string ClusterName { get; set; } = default!;
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }

    public TimeSpan Duration
        => End.HasValue
            ? (End - Start).Value
        : TimeSpan.Zero;

    public string? TaskId { get; set; }
    public string? Status { get; set; }
    public string? Node { get; set; }
    public string? Logs { get; set; }
    public string? Storage { get; set; }
    public virtual ICollection<JobResult> Jobs { get; set; } = [];
}
