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
}
