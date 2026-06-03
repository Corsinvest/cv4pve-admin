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

    private static readonly TimeSpan ChartMaxRange = TimeSpan.FromDays(30);

    private RadzenDataGrid<TaskItemInfo> DataGridRef { get; set; } = default!;
    private ResultLoadData<TaskItemInfo> ResultLoadData { get; set; } = new(null!, -1, null);
    private SearchTextBox<TaskItemInfo>? SearchTextBox { get; set; }
    private IReadOnlyList<string> _allowedModules = [];
    private IEnumerable<FilterDescriptor> _searchFilters = [];

    private List<TasksChart.ChartPoint> ChartData { get; set; } = [];
    private bool ChartVisible { get; set; }
    private bool ChartLoading { get; set; }
    private string? ChartError { get; set; }

    private GridLoader<TaskItem, TaskItemInfo>? _loader;

    protected override void OnInitialized()
    {
        _allowedModules = [.. moduleService.Modules
                                           .Where(a => currentUserService.ClaimsPrincipal!.IsInRole(a.RoleAdmin.Key))
                                           .Select(a => a.Name)];

        taskTracker.Changed += OnTrackerChanged;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            _loader = GridLoader.Create<TaskItem, TaskItemInfo>(DataGridRef, defaultOrderBy: "StartedAt desc, Id desc");

            _loader.WithExtraFilters(() => _searchFilters);

            _loader.OnFiltersChanged(async filters =>
            {
                if (!ChartVisible) { return; }
                ChartLoading = true;
                await InvokeAsync(StateHasChanged);
                await using var db = await dbContextFactory.CreateDbContextAsync();
                await LoadChartDataAsync(db, filters);
                ChartLoading = false;
                await InvokeAsync(StateHasChanged);
            });
        }
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
        if (_loader is null) { return; }
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var query = db.TaskItems
                      .Where(a => a.ModuleName == null || _allowedModules.Contains(a.ModuleName))
                      .Where(a => a.ClusterName == ClusterName, ClusterName is not null);
        ResultLoadData = await _loader.LoadAsync(query,
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
                                                 });
    }

    private async Task LoadChartDataAsync(ModuleDbContext db, List<FilterDescriptor> filters)
    {
        ChartError = null;
        try
        {
            var cutoff = DateTime.UtcNow - ChartMaxRange;
            var query = db.TaskItems.AsQueryable()
                                    .Where(a => a.ModuleName == null || _allowedModules.Contains(a.ModuleName))
                                    .Where(a => a.ClusterName == ClusterName, ClusterName is not null)
                                    .Where(filters, LogicalFilterOperator.And, FilterCaseSensitivity.CaseInsensitive)
                                    .Where(a => a.StartedAt >= cutoff);

            ChartData = await query.GroupBy(a => new { a.StartedAt.Date, a.Status })
                                   .Select(g => new TasksChart.ChartPoint(g.Key.Date, g.Key.Status, g.Count()))
                                   .ToListAsync();
        }
        catch (Exception ex)
        {
            ChartData = [];
            ChartError = ex.Message;
        }
    }

    private async Task ToggleChart()
    {
        ChartVisible = !ChartVisible;
        if (!ChartVisible) { return; }

        ChartLoading = true;
        StateHasChanged();
        await Task.Yield();

        try
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            await LoadChartDataAsync(db, [.. _searchFilters]);
        }
        finally
        {
            ChartLoading = false;
        }
    }

    private Task RefreshAsync()
        => _loader?.RefreshAsync() ?? DataGridRef.Reload();

    private async Task OnSearchFiltersChanged(IEnumerable<FilterDescriptor> filters)
    {
        _searchFilters = filters;
        await RefreshAsync();
    }

    public void Dispose()
    {
        taskTracker.Changed -= OnTrackerChanged;
        _loader?.Dispose();
    }
}
