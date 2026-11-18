/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Components;

public partial class Results(IDbContextFactory<ModuleDbContext> dbContextFactory,
                             DialogService dialogService) : IClusterName, IDisposable
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;
    [Parameter] public bool ShowOnlyError { get; set; }
    [Parameter] public int? JobId { get; set; }
    [Parameter] public string Style { get; set; } = default!;

    private IList<Data> SelectedItems { get; set; } = [];
    private bool _validColumnClick;
    private RadzenDataGrid<Data> DataGridRef { get; set; } = default!;
    private ResultLoadData<Data> ResultLoadData { get; set; } = new(null!, -1, null);
    private GridLoader<JobResult, Data>? _loader;

    private List<DailyChartPoint<string>> ChartData { get; set; } = [];
    private bool ChartVisible { get; set; }
    private bool ChartLoading { get; set; }
    private string? ChartError { get; set; }
    private DailyChartFilter<string>? _filter;

    private string GridStyle => ChartVisible
                                    ? "flex: 1 1 0; min-height: 0;"
                                    : Style;

    private class Data : JobResult;

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            _loader = GridLoader.Create<JobResult, Data>(DataGridRef, defaultOrderBy: "Start desc, Id desc");
        }
    }

    private async Task LoadDataAsync(LoadDataArgs args)
    {
        if (_loader is null) { return; }
        await using var db = await dbContextFactory.CreateDbContextAsync();
        ResultLoadData = await _loader.LoadAsync((await GetQueryAsync(db)).Where(a => a.Start >= _filter!.From && a.Start <= _filter.To,
                                                                                 ChartVisible && _filter != null)
                                                                         .Where(a => a.Job.Label == _filter!.Category, ChartVisible && _filter?.HasCategory == true),
                                                 args,
                                                 a => new Data
                                                 {
                                                     Id = a.Id,
                                                     Job = a.Job,
                                                     SnapName = a.SnapName,
                                                     Status = a.Status,
                                                     Start = a.Start,
                                                     End = a.End
                                                 });
    }

    private async Task<IQueryable<JobResult>> GetQueryAsync(ModuleDbContext db)
    {
        var visible = await ActionHelper.GetVisibleJobIdsAsync(PermissionService, db, ClusterName);
        return db.Results
                 .Where(a => a.Job.Id == JobId, JobId != null)
                 .Where(a => a.Job.ClusterName == ClusterName, JobId == null)
                 .Where(a => visible!.Contains(a.Job.Id), visible != null)
                 .Where(a => !a.Status, ShowOnlyError);
    }

    private async Task ToggleChartAsync()
    {
        ChartVisible = !ChartVisible;
        if (!ChartVisible)
        {
            if (_filter != null)
            {
                _filter = null;
                await (_loader?.RefreshAsync() ?? DataGridRef.Reload());
            }
            return;
        }

        ChartLoading = true;
        ChartError = null;
        StateHasChanged();
        await Task.Yield();

        try
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            var cutoff = DateTime.UtcNow.AddDays(-30);
            ChartData = await (await GetQueryAsync(db)).Where(a => a.Start >= cutoff)
                                                       .GroupBy(a => new { a.Start.Date, a.Job.Label })
                                                       .Select(g => new DailyChartPoint<string>(g.Key.Date, g.Key.Label, g.Count()))
                                                       .ToListAsync();
        }
        catch (Exception ex)
        {
            ChartData = [];
            ChartError = ex.Message;
        }
        finally
        {
            ChartLoading = false;
        }
    }

    private async Task OnChartFilterChangedAsync(DailyChartFilter<string>? filter)
    {
        _filter = filter;
        await (_loader?.RefreshAsync() ?? DataGridRef.Reload());
    }

    public void Dispose() => _loader?.Dispose();

    private void CellClick(DataGridCellMouseEventArgs<Data> e)
        => _validColumnClick = new[] { nameof(Data.SnapName) }.Contains(e.Column!.Property);

    private async Task RowSelectAsync(Data item)
    {
        if (_validColumnClick)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            await dialogService.OpenSideLogAsync(L["Job Log '{0}'", item.Start],
                                                    (await db.Results
                                                             .Where(a => a.Id == item.Id)
                                                             .Select(a => a.Logs)
                                                             .FirstOrDefaultAsync())!);
        }
    }

    private Task KeyDownAsync(KeyboardEventArgs e) => e.HandleGridKeysAsync(SelectedItems.Any(), DeleteAsync, EditSelectedAsync);

    // The check on the clicked column is for the mouse: the key opens the editor of the selected row
    private Task EditSelectedAsync()
    {
        _validColumnClick = true;
        return RowSelectAsync(SelectedItems[0]);
    }

    private async Task DeleteAsync()
    {
        if (!await HasPermissionAsync(ClusterName, Module.Permissions.Results.Delete)) { return; }

        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Delete selected row"], true))
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            await db.Results.DeleteAsync([.. SelectedItems.Select(a => a.Id)]);
            SelectedItems.Clear();
            await (_loader?.RefreshAsync() ?? DataGridRef.Reload());
        }
    }
}
