/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Globalization;
using Corsinvest.ProxmoxVE.Admin.Core.Configuration;
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Core.Models;
using Microsoft.AspNetCore.Components;

namespace Corsinvest.ProxmoxVE.Admin.Module.Profile.Components;

public partial class CulturePreferences : ISettingsParameter<UserSettings>
{
    [Parameter] public UserSettings Settings { get; set; } = default!;
    [Parameter] public EventCallback<UserSettings> SettingsChanged { get; set; }

    private record Culture(string Code, string Name);
    private Culture SelectedCulture { get; set; } = default!;
    private IEnumerable<Culture> AvailableCultures { get; set; } = [];

    protected override void OnInitialized()
    {
        AvailableCultures = [.. ApplicationHelper.SupportedCultures.Select(a => new Culture(a, new CultureInfo(a).NativeName))];
        SelectedCulture = AvailableCultures.FirstOrDefault(a => a.Code == Settings.Culture) ?? AvailableCultures.First();
    }

    private void OnCultureChanged() => Settings.Culture = SelectedCulture.Code;
}
