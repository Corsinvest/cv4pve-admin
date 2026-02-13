/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.Dashboard.Components.Widgets.WebContent;

public class Settings
{
    public ContentType Type { get; set; } = ContentType.Link;
    public string Url { get; set; } = string.Empty;
    public string Icon { get; set; } = "link";
    public string Text { get; set; } = string.Empty;
    public string Html { get; set; } = string.Empty;
}
