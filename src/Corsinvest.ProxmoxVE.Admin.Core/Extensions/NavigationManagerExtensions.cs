/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class NavigationManagerExtensions
{
    public static void ForceReload(this NavigationManager navigationManager)
        => navigationManager.NavigateTo(navigationManager.Uri, forceLoad: true);

    public static void RefreshSignIn(this NavigationManager navigationManager)
        => navigationManager.NavigateToAccount("/RefreshSignIn");

    public static void ForgetTwoFactorBrowser(this NavigationManager navigationManager)
        => navigationManager.NavigateToAccount("/ForgetTwoFactorBrowser");

    private static void NavigateToAccount(this NavigationManager navigationManager, string endpoint)
        => navigationManager.NavigateTo($"{endpoint}?returnUrl={Uri.EscapeDataString(new Uri(navigationManager.Uri).PathAndQuery)}", forceLoad: true);
}
