/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Contracts;
using Corsinvest.AppHero.Core.MudBlazorUI.Extensions;
using Corsinvest.AppHero.Core.MudBlazorUI.Shared.Components.DataGrid;
using Corsinvest.AppHero.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Cluster;

public partial class Resources : IRefreshable
{
    [Parameter] public string Height { get; set; } = default!;
    [Parameter] public bool Multiselect { get; set; }
    [Parameter] public PermissionsRead Permissions { get; set; } = default!;
    [Parameter] public RenderFragment ToolBarContentBefore { get; set; } = default!;
    [Parameter] public RenderFragment ToolBarContentAfter { get; set; } = default!;
    [Parameter] public RenderFragment<CellContext<ClusterResource>> ChildRowContent { get; set; } = default!;
    [Parameter] public IEnumerable<string> PropertiesName { get; set; } = default!;
    [Parameter] public EventCallback<HashSet<ClusterResource>> SelectedItemsChanged { get; set; } = default!;

    [Parameter]
    public Dictionary<string, bool> DefaultSort { get; set; } =
        new()
        {
            [nameof(ClusterResource.Type)] = false,
            [nameof(ClusterResource.Name)] = false,
        };

    [EditorRequired][Parameter] public Func<Task<IEnumerable<ClusterResource>>> GetItems { get; set; } = default!;
    [Parameter] public string NoRecordsContentIcon { get; set; } = Icons.Material.Filled.SentimentDissatisfied;

    [Inject] private IDataGridManager<ClusterResource> DataGridManagerInt { get; set; } = default!;

    public DataGridManager<ClusterResource> DataGridManager => DataGridManagerInt.ToDataGridManager();

    protected override void OnInitialized()
    {
        DataGridManagerInt.Title = L["Vm Not Scheduled"];
        DataGridManagerInt.DefaultSort = DefaultSort;
        DataGridManagerInt.QueryAsync = GetItems;
    }

    public async Task Refresh() => await DataGridManager.Refresh();
    private static Type GetDynamicType(string propertyName) => PveBlazorHelper.AHPropertyColumn.GetDynamicType<ClusterResource>(propertyName);
    private static Dictionary<string, object> GetDynamicParameters(string propertyName) => PveBlazorHelper.AHPropertyColumn.GetDynamicParameters<ClusterResource>(propertyName);
}