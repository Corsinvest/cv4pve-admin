/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Linq.Expressions;
using System.Security.Claims;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;
using Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Layout;

public partial class ActiveTasksMenu(ContextMenuService contextMenuService,
                                     NavigationManager navigationManager,
                                     ITaskTrackerService taskTracker,
                                     ICurrentUserService currentUserService) : IDisposable
{
    private BadgeStyle BadgeStyle { get; set; }
    private string? BadgeText { get; set; }
    private IReadOnlyList<TaskItemInfo> ActiveTasks { get; set; } = [];
    private bool IsMenuOpen { get; set; }
    private ClaimsPrincipal? _principal;
    private MouseEventArgs? _lastMenuArgs;

    protected override async Task OnInitializedAsync()
    {
        _principal = currentUserService.ClaimsPrincipal;
        taskTracker.Changed += OnTrackerChanged;
        taskTracker.AcknowledgedChanged += OnAcknowledgedChanged;
        contextMenuService.OnClose += OnMenuClosed;
        await UpdateBadgeAsync();
    }

    private void OnMenuClosed()
    {
        IsMenuOpen = false;
        var toAck = ActiveTasks.Where(t => t.Status == TaskItemStatus.Failed || t.Status == TaskItemStatus.Abandoned)
                               .Select(t => t.Id)
                               .ToList();
        ActiveTasks = [];
        if (toAck.Count > 0)
        {
            _ = InvokeAsync(async () =>
            {
                await taskTracker.AcknowledgeAsync(toAck);
                await UpdateBadgeAsync();
                StateHasChanged();
            });
        }
    }

    private void OnTrackerChanged(object? sender, TaskItem e)
        => InvokeAsync(async () =>
        {
            await UpdateBadgeAsync();
            if (IsMenuOpen) { await RefreshActiveTasksAsync(e); }
            await InvokeAsync(StateHasChanged);
        });

    private async Task RefreshActiveTasksAsync(TaskItem e)
    {
        var idx = ActiveTasks.ToList().FindIndex(a => a.Id == e.Id);
        if (idx >= 0)
        {
            var list = ActiveTasks.ToList();
            list[idx] = list[idx] with
            {
                Status = e.Status,
                Phase = e.Phase,
                Progress = e.Progress,
                LastLog = e.LastLog,
                EndedAt = e.EndedAt
            };
            ActiveTasks = list;
        }
        else
        {
            ActiveTasks = await GetActiveTasksAsync();
        }

        if (_lastMenuArgs != null)
        {
            contextMenuService.Open(_lastMenuArgs, _ => RenderPanel(ActiveTasks));
        }
    }

    private static readonly Expression<Func<TaskItem, bool>> ActiveFilter = t =>
        t.Status != TaskItemStatus.Completed && (t.EndedAt == null || t.EndedAt >= DateTime.UtcNow.AddHours(-1));

    private async Task UpdateBadgeAsync()
    {
        var tasks = await taskTracker.GetRecentAsync(_principal!,
                                                     a => new { a.Status, a.IsAcknowledged },
                                                     filter: ActiveFilter);
        var runningCount = tasks.Count(t => t.Status == TaskItemStatus.Running);
        var hasUnacknowledgedError = tasks.Any(t => !t.IsAcknowledged
                                                 && (t.Status == TaskItemStatus.Failed
                                                  || t.Status == TaskItemStatus.Abandoned));

        BadgeStyle = hasUnacknowledgedError
                    ? BadgeStyle.Danger
                    : runningCount > 0
                        ? BadgeStyle.Warning
                        : BadgeStyle.Light;

        BadgeText = hasUnacknowledgedError
                    ? tasks.Count(t => !t.IsAcknowledged
                                    && (t.Status == TaskItemStatus.Failed
                                     || t.Status == TaskItemStatus.Abandoned)).ToString()
                    : runningCount > 0
                        ? runningCount.ToString()
                        : null;
    }

    private async Task<IReadOnlyList<TaskItemInfo>> GetActiveTasksAsync()
        => await taskTracker.GetRecentAsync(_principal!,
                                            a => new TaskItemInfo(a.Id,
                                                                  a.Title,
                                                                  a.ClusterName,
                                                                  a.ModuleName,
                                                                  a.DetailUrl,
                                                                  a.IsCancellable,
                                                                  a.Status,
                                                                  a.Phase,
                                                                  a.StartedAt,
                                                                  a.EndedAt,
                                                                  a.CreatedBy,
                                                                  a.Progress,
                                                                  a.LastLog),
                                            filter: ActiveFilter);

    private async Task ShowTasksMenu(MouseEventArgs args)
    {
        args.ClientY += 24;
        IsMenuOpen = true;
        _lastMenuArgs = args;

        ActiveTasks = await GetActiveTasksAsync();
        contextMenuService.Open(args, _ => RenderPanel(ActiveTasks));
    }

    private void OnAfterAction()
    {
        IsMenuOpen = false;
        ActiveTasks = [];
        _lastMenuArgs = null;
        contextMenuService.Close();
    }

    private void ViewAllTasks()
    {
        contextMenuService.Close();
        navigationManager.NavigateTo(UrlHelper.SystemTasksUrl);
    }

    private void OnAcknowledgedChanged(object? sender, EventArgs e)
        => InvokeAsync(async () =>
        {
            await UpdateBadgeAsync();
            StateHasChanged();
        });

    public void Dispose()
    {
        taskTracker.Changed -= OnTrackerChanged;
        taskTracker.AcknowledgedChanged -= OnAcknowledgedChanged;
        contextMenuService.OnClose -= OnMenuClosed;
    }
}
