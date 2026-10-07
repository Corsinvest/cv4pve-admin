/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Settings;

public class SettingSection<T>
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "settings";
    public Type ComponentType { get; set; } = default!;
    public Func<IServiceScope, T, Task>? OnSavedAsync { get; set; }

    // Address to reload after the save, given the settings and the local address of the current page.
    // The component navigates: the scope passed to OnSavedAsync is a new one, with no circuit
    public Func<T, string, string>? GetRedirectUrl { get; set; }
}
