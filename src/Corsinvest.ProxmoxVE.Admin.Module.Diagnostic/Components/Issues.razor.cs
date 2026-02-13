/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Components;

public partial class Issues(IDbContextFactory<ModuleDbContext> dbContextFactory,
                            DialogService dialogService) : IClusterName, IRefreshableData
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private RadzenDataGrid<IgnoredIssue> DataGridRef { get; set; } = default!;
    private IList<IgnoredIssue> SelectedItems { get; set; } = [];
    private ResultLoadData<IgnoredIssue> ResultLoadData { get; set; } = new(null!, 0, null);
    private bool _validColumnClick;

    private async Task LoadDataAsync(LoadDataArgs args)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        ResultLoadData = await DataGridRef.LoadDataAsync(db.IgnoredIssues.FromClusterName(ClusterName),
                                                         args,
                                                         a => a,
                                                         ResultLoadData.Filter);
    }

    public async Task RefreshDataAsync() => await DataGridRef.Reload();

    private async Task KeyDownAsync(KeyboardEventArgs e)
    {
        if (SelectedItems.Any() && e.IsForDelete()) { await DeleteAsync(); }
        else if (SelectedItems.Any() && e.IsForEdit()) { await RowSelectAsync(SelectedItems[0]); }
        else if (e.IsForNew()) { }
    }

    private async Task RowSelectAsync(IgnoredIssue item)
    {
        if (_validColumnClick) { await ShowEditorAsync(item); }
    }

    private void CellClick(DataGridCellMouseEventArgs<IgnoredIssue> e)
        => _validColumnClick = new[] { nameof(IgnoredIssue.IdResource) }.Contains(e.Column!.Property);

    private async Task DeleteAsync()
    {
        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Delete selected row"], true))
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            await db.IgnoredIssues.DeleteAsync(SelectedItems[0].Id);
            await DataGridRef.Reload();
        }
    }

    //private async Task AddAsync() => await ShowEditorAsync(new(), true);

    private async Task ShowEditorAsync(IgnoredIssue item)
    {
        var isNew = item.Id == 0;
        var title = isNew
                        ? L["New"]
                        : L["Edit {0}", item.IdResource!];

        if (await dialogService.OpenSideEditAsync<IssuesDialog>(title, isNew, item) != null)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            await db.AddOrUpdateAsync(item);

            await DataGridRef.Reload();
        }
    }
}
