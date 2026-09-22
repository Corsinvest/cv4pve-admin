/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.DataGrid;

/// <summary>
/// Keeps a grid's group rows collapsed or expanded across re-renders.
///
/// <para>RadzenDataGrid records which groups are <em>collapsed</em>, keyed on the group row
/// object it rebuilds on every render. After a refresh — or a scroll, with virtualization —
/// that dictionary no longer matches anything, and because the test is "not in the collapsed
/// list, therefore expanded", every group springs open. <c>AllGroupsExpanded</c> does not help:
/// it applies on the first render only.</para>
///
/// <para>This holds the group keys instead, which survive a refresh, and writes the answer back
/// on every render. State lives as long as the component: navigate away and back and the groups
/// return to their default.</para>
/// </summary>
/// <param name="collapsedByDefault">Whether groups start collapsed. Long reports are easier to
/// take in as a list of headings.</param>
public sealed class GroupCollapseState(bool collapsedByDefault = true)
{
    private readonly HashSet<string> _collapsed = [];
    private readonly HashSet<string> _seen = [];

    // Nested grouping puts a label and a VM id in the same set, so the level goes into the key:
    // a label "100" and guest 100 are different groups.
    private static string? KeyOf(Group? group)
        => group?.Data.Key is { } key ? $"{group.Level}:{key}" : null;

    /// <summary>Wire to <c>GroupRowRender</c>.</summary>
    public void OnRender(GroupRowRenderEventArgs args)
    {
        if (KeyOf(args.Group) is not { } key) { return; }

        // The default applies to any group not seen before, rather than on args.FirstRender: the
        // grid clears that flag in OnAfterRenderAsync, and on the first pass it has no rows yet
        // — data arrives later — so no group row is ever rendered while the flag is still set.
        // This also covers a group that appears later, and a change of grouping column.
        if (collapsedByDefault && _seen.Add(key)) { _collapsed.Add(key); }

        // Written on every render, never left null: null means "let the grid decide", and what
        // the grid decides is exactly the behaviour being corrected here.
        args.Expanded = !_collapsed.Contains(key);
    }

    /// <summary>Wire to <c>GroupRowExpand</c>.</summary>
    public void OnExpand(Group group)
    {
        if (KeyOf(group) is { } key) { _collapsed.Remove(key); }
    }

    /// <summary>Wire to <c>GroupRowCollapse</c>.</summary>
    public void OnCollapse(Group group)
    {
        if (KeyOf(group) is { } key) { _collapsed.Add(key); }
    }
}
