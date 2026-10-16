/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class WebExtensions
{
    public static bool IsForDelete(this KeyboardEventArgs args) => args.Key == "Delete";
    public static bool IsForEdit(this KeyboardEventArgs args) => args.Key == "F2";
    public static bool IsForNew(this KeyboardEventArgs args) => args.Key is "N" or "n" && args.CtrlKey;

    // Delete, F2 and Ctrl+N of the grids: a key without an action does nothing
    public static async Task HandleGridKeysAsync(this KeyboardEventArgs args,
                                                 bool hasSelection,
                                                 Func<Task>? delete,
                                                 Func<Task>? edit = null,
                                                 Func<Task>? add = null)
    {
        var action = hasSelection && args.IsForDelete() ? delete
                     : hasSelection && args.IsForEdit() ? edit
                     : args.IsForNew() ? add
                     : null;
        if (action != null) { await action(); }
    }
}
