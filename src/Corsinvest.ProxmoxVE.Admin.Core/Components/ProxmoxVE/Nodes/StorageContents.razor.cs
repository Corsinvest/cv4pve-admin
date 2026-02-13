/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Nodes;

public partial class StorageContents<TItem> where TItem : NodeStorageContent
{
    [Parameter] public string Style { get; set; } = default!;
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public IEnumerable<TItem> Items { get; set; } = [];
    [Parameter] public IEnumerable<string> OrderBy { get; set; } = [];
    [Parameter] public IEnumerable<GroupDescriptor> Groups { get; set; } = [];

    [Parameter]
    public IEnumerable<string> PropertiesName { get; set; } =
    [
        nameof(NodeStorageContent.Storage),
        nameof(NodeStorageContent.VmId),
        nameof(NodeStorageContent.FileName),
        nameof(NodeStorageContent.Size),
        nameof(NodeStorageContent.CreationDate),
        nameof(NodeStorageContent.Format),
        nameof(NodeStorageContent.Verified),
        nameof(NodeStorageContent.Encrypted)
    ];

    private void OnRender(DataGridRenderEventArgs<TItem> args)
    {
        if (args.FirstRender)
        {
            args.Grid!.Groups.AddRange(Groups);
            StateHasChanged();
        }
    }

    private static void OnGroupRowRender(GroupRowRenderEventArgs args)
    {
        if (args.FirstRender)
        {
            args.Expanded = false;
        }
    }

    private static FilterMode? GetFilterMode(string propertyName)
        => propertyName switch
        {
            nameof(NodeStorageContent.Storage)
            or nameof(NodeStorageContent.VmId)
            or nameof(NodeStorageContent.Content)
            or nameof(NodeStorageContent.Format) => (FilterMode?)FilterMode.CheckBoxList,
            _ => null
        };

    private int? GetOrderIndex(string propertyName)
    {
        var index = OrderBy.ToArray().IndexOf(propertyName) + 1;
        return index > 0 ? index : null;
    }
}
