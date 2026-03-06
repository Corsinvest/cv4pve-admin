/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;

public sealed class TaskScope(TaskItem item) : IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();
    public TaskItem Item { get; } = item;
    public CancellationToken CancellationToken => _cts.Token;
    public void RequestCancel() => _cts.Cancel();
    public void Log(string message, LogLevel level = LogLevel.Information) => Item.AddLog(message, level);

    public void SetProgress(int current, int total)
    {
        Item.Progress = total > 0
            ? (int)Math.Round(current * 100.0 / total)
            : null;
        Item.NotifyUpdated();
    }

    public void LogProgress(int current, int total, string message, string? phase = null, LogLevel level = LogLevel.Information)
    {
        Item.Progress = total > 0
            ? (int)Math.Round(current * 100.0 / total)
            : null;
        Item.Phase = phase ?? message;
        Item.AddLog(message, level);
    }

    public async ValueTask DisposeAsync()
    {
        Item.EndedAt = DateTime.UtcNow;

        if (Item.Status == TaskItemStatus.Running)
        {
            Item.Status = _cts.IsCancellationRequested
                            ? TaskItemStatus.Cancelled
                            : TaskItemStatus.Completed;
        }

        _cts.Dispose();
        Item.NotifyEnded();
    }
}
