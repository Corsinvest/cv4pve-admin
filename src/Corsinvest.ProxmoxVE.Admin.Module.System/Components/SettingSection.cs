/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Components;

public class SettingSection
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "settings";
    public Type ComponentType { get; set; } = default!;
}
