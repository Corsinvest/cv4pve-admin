/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class NavigationManagerExtensions
{
    public static void ForceReload(this NavigationManager navigationManager)
        => navigationManager.NavigateTo(navigationManager.Uri, forceLoad: true);
}
