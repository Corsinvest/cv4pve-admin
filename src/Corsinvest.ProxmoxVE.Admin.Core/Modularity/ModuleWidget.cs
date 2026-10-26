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

    // Counts or totals of every VM, node or storage of a cluster: shown only on the clusters
    // where the user can read all of them
    public bool ClusterWide { get; set; }

    public bool IsSubscriptionRequired => RenderInfo?.Type == typeof(Components.SubscriptionRequired);

    // A widget added by the Enterprise Edition: in the Community Edition it is listed marked, with no data
    public static IEnumerable<ModuleWidget> EnterprisePlaceholder(ModuleBase module,
                                                                  string name,
                                                                  string description,
                                                                  int width,
                                                                  int height,
                                                                  bool clusterWide = true)
        => BuildInfo.IsEnterpriseEdition
            ? []
            : [new(module, name)
               {
                   ClusterWide = clusterWide,
                   Description = description,
                   RenderInfo = new(typeof(Components.SubscriptionRequired)),
                   Width = width,
                   Height = height
               }];
}
