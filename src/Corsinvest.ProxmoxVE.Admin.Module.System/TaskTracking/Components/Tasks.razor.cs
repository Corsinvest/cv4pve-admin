/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.TaskTracking.Components;

public partial class Tasks(ITaskTrackerService taskTracker,
                           IModuleService moduleService,
                           ICurrentUserService currentUserService,
                           IDbContextFactory<ModuleDbContext> dbContextFactory) : IDisposable
{
    [Parameter] public string? ClusterName { get; set; }

    private RadzenDataGrid<TaskItemInfo> DataGridRef { get; set; } = default!;
    private ResultLoadData<TaskItemInfo> ResultLoadData { get; set; } = new(null!, -1, null);
    private IReadOnlyList<string> _allowedModules = [];

    protected override void OnInitialized()
    {
        _allowedModules = [.. moduleService.Modules
                                           .Where(a => currentUserService.ClaimsPrincipal!.IsInRole(a.RoleAdmin.Key))
                                           .Select(a => a.Name)];

        taskTracker.Changed += OnTrackerChanged;
    }

    private void OnTrackerChanged(object? sender, TaskItem e)
    {
        var data = ResultLoadData.Data;
        var idx = data?.FindIndex(a => a.Id == e.Id) ?? -1;
        if (idx >= 0)
        {
            data![idx].Status = e.Status;
            data[idx].Phase = e.Phase;
            data[idx].Progress = e.Progress;
            data[idx].LastLog = e.LastLog;
            data[idx].EndedAt = e.EndedAt;
            _ = InvokeAsync(StateHasChanged);
        }
        else
        {
            _ = InvokeAsync(DataGridRef.Reload);
        }
    }

    private async Task LoadDataAsync(LoadDataArgs args)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();

        ResultLoadData = await DataGridRef.LoadDataAsync(db.TaskItems
                                                           .Where(a => a.ModuleName == null || _allowedModules.Contains(a.ModuleName))
                                                           .Where(a => a.ClusterName == ClusterName, ClusterName is not null),
                                                         args,
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
                                                         ResultLoadData.Filter);
    }

    private Task RefreshAsync() => DataGridRef.Reload();
    public void Dispose() => taskTracker.Changed -= OnTrackerChanged;
}
