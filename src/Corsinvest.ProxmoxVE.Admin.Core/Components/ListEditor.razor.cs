/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

/// <summary>
/// Editable list with always-open rows: add, remove and, optionally, move up/down.
/// The row fields come from <see cref="ItemTemplate"/>; the list is changed in place.
/// </summary>
public partial class ListEditor<TItem>
{
    [Parameter, EditorRequired] public IList<TItem> Items { get; set; } = default!;
    [Parameter, EditorRequired] public RenderFragment<TItem> ItemTemplate { get; set; } = default!;
    [Parameter, EditorRequired] public Func<TItem> NewItem { get; set; } = default!;

    /// <summary>Shows up/down buttons: use it when the order matters (e.g. priority).</summary>
    [Parameter] public bool AllowReorder { get; set; }

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? AddText { get; set; }

    /// <summary>Extra buttons next to Add.</summary>
    [Parameter] public RenderFragment? ToolbarContent { get; set; }

    /// <summary>Raised after a row is added, removed or moved.</summary>
    [Parameter] public EventCallback Changed { get; set; }

    private Task AddAsync()
    {
        Items.Add(NewItem());
        return Changed.InvokeAsync();
    }

    private Task RemoveAsync(TItem item)
    {
        Items.Remove(item);
        return Changed.InvokeAsync();
    }

    private Task MoveAsync(int index, int offset)
    {
        var target = index + offset;
        if (target < 0 || target >= Items.Count) { return Task.CompletedTask; }

        (Items[index], Items[target]) = (Items[target], Items[index]);
        return Changed.InvokeAsync();
    }
}
