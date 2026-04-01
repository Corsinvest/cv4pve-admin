/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components.Widgets.ResourcesUsage;

public class Settings
{
    public DataGridSettings DataGridSettings { get; set; } = new();
    public bool UseProgressBarPercentage { get; set; } = true;
    public bool DescriptionAsLink { get; set; } = true;
    public ResourcesViewPropertyIconStatus PropertyIconStatus { get; set; } = ResourcesViewPropertyIconStatus.None;
    public ResourceColumnIconStatus IconStatus { get; set; } = ResourceColumnIconStatus.IconAndText;
}
