/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.JSInterop;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Components.ClusterConfig;

public partial class NodesSettings(IAdminService adminService,
                                   DialogService dialogService,
                                   NotificationService notificationService,
                                   IJSRuntime jSRuntime) : IModelParameter<ClusterSettings>
{
    [Parameter, EditorRequired] public ClusterSettings Model { get; set; } = default!;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string Style { get; set; } = default!;

    private RadzenDataGrid<ClusterNodeSettings> DataGridRef { get; set; } = default!;
    private bool InEdit { get; set; }
    private ClusterNodeSettings _draggedItem = default!;

    private async Task EditRowAsync(ClusterNodeSettings item)
    {
        await DataGridRef.EditRow(item);
        InEdit = true;
    }

    private void OnCreateRow(ClusterNodeSettings item)
    {
        Model.Nodes.Add(item);
        InEdit = false;
    }

    private async Task SaveRowAsync(ClusterNodeSettings item)
    {
        await DataGridRef.UpdateRow(item);
        await DataGridRef.Reload();
        InEdit = false;
    }

    private void CancelEdit(ClusterNodeSettings item)
    {
        DataGridRef.CancelEditRow(item);
        InEdit = false;
    }

    private async Task InsertRowAsync()
    {
        await DataGridRef.InsertRow(new ClusterNodeSettings());
        InEdit = true;
    }

    private async Task DeleteRowAsync(ClusterNodeSettings item)
    {
        Model.Nodes.Remove(item);
        await DataGridRef.Reload();
    }

    public async Task DiscoverAsync()
    {
        await PveAdminUIHelper.PopulateClusterSettingsAsync(adminService, Model, dialogService, notificationService, L);
        await DataGridRef.Reload();
    }

    private void RowRender(RowRenderEventArgs<ClusterNodeSettings> args)
    {
        args.Attributes.Add("title", "Drag row to reorder");
        args.Attributes.Add("style", "cursor:grab");
        args.Attributes.Add("draggable", "true");
        args.Attributes.Add("ondragover", "event.preventDefault();event.target.closest('.rz-data-row').classList.add('my-class')");
        args.Attributes.Add("ondragleave", "event.target.closest('.rz-data-row').classList.remove('my-class')");
        args.Attributes.Add("ondragstart", EventCallback.Factory.Create<DragEventArgs>(this, () => _draggedItem = args.Data!));
        args.Attributes.Add("ondrop", EventCallback.Factory.Create<DragEventArgs>(this, async () =>
        {
            var draggedIndex = Model.Nodes.IndexOf(_draggedItem!);
            var droppedIndex = Model.Nodes.IndexOf(args.Data!);
            Model.Nodes.Remove(_draggedItem);
            Model.Nodes.Insert(draggedIndex <= droppedIndex ? droppedIndex++ : droppedIndex, _draggedItem);

            await jSRuntime.InvokeVoidAsync("eval", "document.querySelector('.my-class').classList.remove('my-class')");
            await DataGridRef.Reload();
        }));
    }
}
