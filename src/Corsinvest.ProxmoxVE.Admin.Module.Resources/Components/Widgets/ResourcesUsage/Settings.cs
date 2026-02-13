/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components.Widgets.ResourcesUsage;

public class Settings
{
    public DataGridSettings DataGridSettings { get; set; } = new();
    public bool UseProgressBarPercentage { get; set; } = true;
    public bool DescriptionAsLink { get; set; } = true;
    public ResourcesExPropertyIconStatus PropertyIconStatus { get; set; } = ResourcesExPropertyIconStatus.None;
    public ResourceColumnIconStatus IconStatus { get; set; } = ResourceColumnIconStatus.IconAndText;
}
