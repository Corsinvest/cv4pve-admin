/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;

public class TaskItem : IId, IClusterName
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string? ModuleName { get; set; }
    public string ClusterName { get; set; } = default!;
    public string? ReferenceId { get; set; }
    public string? DetailUrl { get; set; }
    public bool IsCancellable { get; set; } = true;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Running;
    public string? Phase { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public TimeSpan TimeoutAfter { get; set; } = TimeSpan.FromHours(1);
    public int? Progress { get; set; }

    public string CreatedBy { get; set; } = default!;
    public string? LastLog { get; set; }
    public bool IsAcknowledged { get; set; }

    public List<string> Logs { get; set; } = [];

    public event Action? Updated;
    public event Action? TaskEnded;

    public void AddLog(string message, LogLevel level = LogLevel.Information)
    {
        LastActivity = DateTime.UtcNow;
        LastLog = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
        Logs.Add(LastLog);
        Updated?.Invoke();
    }

    internal void NotifyUpdated() => Updated?.Invoke();
    internal void NotifyEnded() => TaskEnded?.Invoke();
}
