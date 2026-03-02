/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Components.Settings;

namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class SettingsHelper
{
    public static RenderComponentInfo CreateSettingsAccordion<TSettings>(IEnumerable<SettingSection<TSettings>> sections, bool forCurrentUser = false)
        => new(typeof(SettingsAccordion<TSettings>),
               new Dictionary<string, object>
               {
                   [nameof(SettingsAccordion<>.Sections)] = sections,
                   [nameof(SettingsAccordion<>.ForCurrentUser)] = forCurrentUser
               });

}
