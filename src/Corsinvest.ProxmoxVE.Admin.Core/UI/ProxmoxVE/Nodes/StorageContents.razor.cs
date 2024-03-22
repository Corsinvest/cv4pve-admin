/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.MudBlazorUI.Shared.Components.DataGrid;
using Corsinvest.AppHero.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Nodes;

public partial class StorageContents
{
    [Parameter] public string Height { get; set; } = default!;
    [EditorRequired][Parameter] public Func<Task<IEnumerable<NodeStorageContent>>> GetItems { get; set; } = default!;
    [Parameter] public string GroupBy { get; set; } = nameof(NodeStorageContent.Storage);
    [Parameter] public PermissionsRead Permissions { get; set; } = default!;

    [Parameter]
    public IEnumerable<string> PropertiesName { get; set; } =
    [
        nameof(NodeStorageContent.Storage),
        nameof(NodeStorageContent.VmId),
        nameof(NodeStorageContent.FileName),
        nameof(NodeStorageContent.Size),
        nameof(NodeStorageContent.Creation),
        nameof(NodeStorageContent.Format),
        nameof(NodeStorageContent.Verified),
        nameof(NodeStorageContent.Encrypted),
    ];

    [Inject] private IDataGridManager<NodeStorageContent> DataGridManager { get; set; } = default!;

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Resources"];
        DataGridManager.DefaultSort = new()
        {
            [nameof(NodeStorageContent.Storage)] = false,
            [nameof(NodeStorageContent.VmId)] = false,
            [nameof(NodeStorageContent.FileName)] = false,
        };
        DataGridManager.QueryAsync = GetItems;
    }

    private static Type GetDynamicType(string propertyName) => PveBlazorHelper.AHPropertyColumn.GetDynamicType<NodeStorageContent>(propertyName);
    private Dictionary<string, object> GetDynamicParameters(string propertyName)
    {
        var dic = PveBlazorHelper.AHPropertyColumn.GetDynamicParameters<NodeStorageContent>(propertyName);
        dic.Add(nameof(AHPropertyColumn<NodeStorageContent, object>.Groupable), true);
        dic.Add(nameof(AHPropertyColumn<NodeStorageContent, object>.Grouping), propertyName == GroupBy);
        dic.Add(nameof(AHPropertyColumn<NodeStorageContent, object>.GroupTemplate), GroupTemplateRender(propertyName));
        return dic;
    }
}