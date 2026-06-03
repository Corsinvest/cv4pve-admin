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

    private static string StatusColor(TaskItemStatus status) => status switch
    {
        TaskItemStatus.Running => "#1e88e5",       // blue
        TaskItemStatus.Completed => "#43a047",     // green
        TaskItemStatus.Failed => "#e53935",        // red
        TaskItemStatus.Cancelled => "#757575",     // grey
        TaskItemStatus.Abandoned => "#fb8c00",     // orange
        _ => "#9e9e9e"
    };
}
