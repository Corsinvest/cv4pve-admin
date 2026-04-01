/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Security.Claims;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;
using Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Layout;

public partial class ActiveTasksMenu(ContextMenuService contextMenuService,
                                     ITaskTrackerService taskTracker,
                                     ICurrentUserService currentUserService) : IDisposable
{
    private BadgeStyle BadgeStyle { get; set; }
    private string? BadgeText { get; set; }

    private ClaimsPrincipal? _principal;

    protected override async Task OnInitializedAsync()
    {
        _principal = currentUserService.ClaimsPrincipal;

        taskTracker.Changed += OnChanged;
        taskTracker.AcknowledgedChanged += OnAcknowledgedChanged;

        await UpdateBadgeAsync();
    }

    private async Task ShowTasksMenu(MouseEventArgs args)
    {
        args.ClientY += 24;
        contextMenuService.Open(args, _ => RenderPanel);
    }

    private void OnChanged(object? sender, TaskItem e) => RefreshBadge();
    private void OnAcknowledgedChanged(object? sender, EventArgs e) => RefreshBadge();

    private void RefreshBadge()
        => InvokeAsync(async () =>
        {
            await UpdateBadgeAsync();
            StateHasChanged();
        });

    private async Task UpdateBadgeAsync()
    {
        var tasks = await taskTracker.GetRecentAsync(
            _principal!,
            a => new { a.Status, a.IsAcknowledged },
            filter: TaskItemFilters.Active);

        var runningCount = tasks.Count(t => t.Status == TaskItemStatus.Running);

        var hasUnacknowledgedError = tasks.Any(t =>
            !t.IsAcknowledged &&
            (t.Status == TaskItemStatus.Failed || t.Status == TaskItemStatus.Abandoned));

        BadgeStyle = hasUnacknowledgedError
            ? BadgeStyle.Danger
            : runningCount > 0
                ? BadgeStyle.Warning
                : BadgeStyle.Light;

        BadgeText = hasUnacknowledgedError
            ? tasks.Count(t =>
                !t.IsAcknowledged &&
                (t.Status == TaskItemStatus.Failed || t.Status == TaskItemStatus.Abandoned))
                .ToString()
            : runningCount > 0
                ? runningCount.ToString()
                : null;
    }

    public void Dispose()
    {
        taskTracker.Changed -= OnChanged;
        taskTracker.AcknowledgedChanged -= OnAcknowledgedChanged;
    }
}
