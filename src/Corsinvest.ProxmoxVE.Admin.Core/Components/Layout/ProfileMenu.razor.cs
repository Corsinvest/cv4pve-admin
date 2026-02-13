/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Layout;

public partial class ProfileMenu(ThemeService themeService,
                                 ICurrentUserService currentUserService,
#pragma warning disable CS9113 
                                 CookieThemeService cookieThemeService) : IDisposable
#pragma warning restore CS9113 
{
    [Parameter] public IEnumerable<ModuleLinkBase> Links { get; set; } = [];

    private string DisplayName { get; set; } = default!;
    private string Email { get; set; } = default!;
    private string? ProfileImageUrl { get; set; }

    protected override async Task OnInitializedAsync()
    {
        themeService.ThemeChanged += OnThemeChanged;

        var user = await currentUserService.GetUserAsync();
        DisplayName = user!.DisplayName!;
        Email = user!.Email!;
        ProfileImageUrl = user!.ProfileImageUrl;

        //await RefreshDataAsync();
    }

    private bool IsDark => themeService.Theme!.EndsWith("-dark");

    private void OnClick(RadzenProfileMenuItem item)
    {
        if (item.Value == "Theme")
        {
            themeService.SetTheme(ApplicationHelper.ThemeName + (!IsDark ? "-dark" : string.Empty));
        }
    }

    private void OnThemeChanged() => StateHasChanged();
    public void Dispose() => themeService.ThemeChanged -= OnThemeChanged;
}
