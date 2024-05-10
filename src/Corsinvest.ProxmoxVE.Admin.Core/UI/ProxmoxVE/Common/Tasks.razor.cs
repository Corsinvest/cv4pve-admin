/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Contracts;
using Corsinvest.AppHero.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;
using Nextended.Core.Extensions;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Common;

public partial class Tasks : IRefreshable
{
    [EditorRequired][Parameter] public Func<Task<IEnumerable<NodeTask>>> GetItems { get; set; } = default!;
    //[EditorRequired][Parameter] public Func<Task<string>,NodeTask> GetTaskLog { get; set; } = default!;
    [EditorRequired][Parameter] public PveClient PveClient { get; set; } = default!;
    [Parameter] public PermissionsRead Permissions { get; set; } = default!;
    [Parameter] public EventCallback<bool> OnlyErrors { get; set; } = default!;

    [Inject] private IDataGridManager<NodeTask> DataGridManager { get; set; } = default!;

    public async Task RefreshAsync() => await DataGridManager.RefreshAsync();

    [Parameter]
    public IEnumerable<string> PropertiesName { get; set; } =
    [
        nameof(NodeTask.StartTimeDate),
        nameof(NodeTask.EndTimeDate),
        nameof(NodeTask.DurationInfo),
        nameof(NodeTask.DescriptionFull),
        nameof(NodeTask.Status),
        nameof(NodeTask.User)
    ];

    private bool DialogVisible { get; set; }
    private string Logs { get; set; } = default!;

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Tasks"];
        DataGridManager.DefaultSort = new() { [nameof(NodeTask.StartTime)] = true };
        DataGridManager.QueryAsync = GetItems;
    }

    private async Task TaskShowDetailLogAsync()
    {
        Logs = await GetTaskLog(DataGridManager.SelectedItem);
        DialogVisible = true;
    }

    private string RowClassFunc(NodeTask item, int rowNumber)
    {
        var css = string.Empty;
        if (DataGridManager.SelectedItem == item)
        {
            css = "selected";
        }
        else
        {
            if (item.Status == "running" || string.IsNullOrWhiteSpace(item.Status)) { }
            else if (!item.StatusOk) { css = "mud-theme-error"; }
        }
        return css;
    }

    private async Task RowClick(DataGridRowClickEventArgs<NodeTask> arg)
    {
        if (arg.MouseEventArgs.Detail == 2) { await TaskShowDetailLogAsync(); }
    }

    private async Task<string> GetTaskLog(NodeTask item)
        => (await PveClient.Nodes[item.Node].Tasks[item.UniqueTaskId].Log.GetAsync(10000))
                .JoinAsString(Environment.NewLine);

    private static Type GetDynamicType(string propertyName) => PveBlazorHelper.AHPropertyColumn.GetDynamicType<NodeTask>(propertyName);

    private string CellClassFunc(NodeTask item)
        => item.StatusOk || DataGridManager.SelectedItem == item
            ? string.Empty
            : "mud-theme-error";

    private Dictionary<string, object> GetDynamicParameters(string propertyName)
    {
        var dic = PveBlazorHelper.AHPropertyColumn.GetDynamicParameters<NodeTask>(propertyName);
        dic.Add("CellClassFunc", CellClassFunc);
        return dic;
    }
}