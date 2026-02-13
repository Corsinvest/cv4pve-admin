/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;

namespace Corsinvest.ProxmoxVE.Admin.Module.AIServer;

public class Settings : IModuleSettings
{
    [Required] public string ClusterName { get; set; } = default!;

    [Display(Name = "Enable MCP Server")]
    public bool Enabled { get; set; }

    [Encrypt]
    [Required]
    [Display(Name = "API Token")]
    public string ApiToken { get; set; } = string.Empty;
}
