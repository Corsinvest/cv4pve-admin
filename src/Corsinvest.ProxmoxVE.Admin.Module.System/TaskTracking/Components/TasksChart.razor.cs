/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.TaskTracking.Components;

public partial class TasksChart
{
    public record ChartPoint(DateTime Day, TaskItemStatus Status, int Count);

    [Parameter] public List<ChartPoint> Data { get; set; } = [];
    [Parameter] public string? ErrorMessage { get; set; }
    [Parameter] public bool Loading { get; set; }

    private static readonly TaskItemStatus[] _statuses =
    [
        TaskItemStatus.Running,
        TaskItemStatus.Completed,
        TaskItemStatus.Failed,
        TaskItemStatus.Cancelled,
        TaskItemStatus.Abandoned,
    ];

    private List<ChartPoint>? _lastData;
    private string? _lastError;
    private bool _lastLoading;

    protected override bool ShouldRender()
    {
        if (ReferenceEquals(_lastData, Data) && _lastError == ErrorMessage && _lastLoading == Loading) { return false; }
        _lastData = Data;
        _lastError = ErrorMessage;
        _lastLoading = Loading;
        return true;
    }

    // Theme colours rather than fixed hex: the chart then follows the theme and agrees with the
    // badges and tinted rows elsewhere. These were Material greens and reds in a Fluent product.
    private static string StatusColor(TaskItemStatus status) => status switch
    {
        TaskItemStatus.Running => "var(--cv4pve-severity-running)",
        TaskItemStatus.Completed => "var(--rz-success)",
        TaskItemStatus.Failed => "var(--rz-danger)",
        TaskItemStatus.Cancelled => "var(--cv4pve-severity-muted)",
        TaskItemStatus.Abandoned => "var(--rz-warning)",
        _ => "var(--cv4pve-severity-disabled)"
    };
}
