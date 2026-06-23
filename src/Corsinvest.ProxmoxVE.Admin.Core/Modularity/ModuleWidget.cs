/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

namespace Corsinvest.ProxmoxVE.Admin.Core.Modularity;

public class ModuleWidget
{
    public ModuleWidget(ModuleBase module, string name)
    {
        Module = module;
        Name = name;
        Permission = new(module.PermissionWidgetBaseKey, Name, Name);
    }

    public RenderComponentInfo RenderInfo { get; set; } = default!;
    public RenderSettingsInfo? RenderSettingsInfo { get; set; }

    public ModuleBase Module { get; }
    public string Name { get; }
    public int Height { get; set; } = 1;
    public int Width { get; set; } = 1;
    public string Description { get; set; } = default!;
    public Permission Permission { get; }

    /// <summary>
    /// Whether to include this widget in the module Overview page.
    /// Default is <c>true</c>: opt out (set <c>false</c>) for widgets that are too heavy
    /// or too specialised to be useful in the fixed mini-dashboard.
    /// </summary>
    public bool ShowInOverview { get; set; } = true;
}
