/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Security.Claims;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;
using Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Layout;

public partial class ActiveTasksPanel(ITaskTrackerService taskTracker,
                                      ICurrentUserService currentUserService,
                                      NavigationManager navigationManager,
                                      ContextMenuService contextMenuService) : IDisposable
{
    private IReadOnlyList<TaskItemInfo> Tasks { get; set; } = [];
    private ClaimsPrincipal? _principal;

    protected override async Task OnInitializedAsync()
    {
        _principal = currentUserService.ClaimsPrincipal;
        taskTracker.Changed += OnChanged;
        Tasks = await LoadTasks();
    }

    private void OnChanged(object? sender, TaskItem e)
        => InvokeAsync(async () =>
        {
            Tasks = await LoadTasks();
            StateHasChanged();
        });

    private Task<IReadOnlyList<TaskItemInfo>> LoadTasks()
        => taskTracker.GetRecentAsync(_principal!,
                                      a => new TaskItemInfo
                                      {
                                          Id = a.Id,
                                          Title = a.Title,
                                          ClusterName = a.ClusterName,
                                          ModuleName = a.ModuleName,
                                          DetailUrl = a.DetailUrl,
                                          IsCancellable = a.IsCancellable,
                                          Status = a.Status,
                                          Phase = a.Phase,
                                          StartedAt = a.StartedAt,
                                          EndedAt = a.EndedAt,
                                          CreatedBy = a.CreatedBy,
                                          Progress = a.Progress,
                                          LastLog = a.LastLog,
                                      },
                                      filter: TaskItemFilters.Active);

    private void OnAfterAction() => contextMenuService.Close();

    private void ViewAll()
    {
        contextMenuService.Close();
        navigationManager.NavigateTo(UrlHelper.SystemTasksUrl);
    }

    public void Dispose() => taskTracker.Changed -= OnChanged;
}
