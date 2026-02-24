/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Module.AIServer.Helpers;

namespace Corsinvest.ProxmoxVE.Admin.Module.AIServer;

public class Settings : IModuleSettings
{
    [Required] public string ClusterName { get; set; } = ApplicationHelper.AllClusterName;

    public bool Enabled { get; set; }
    public ToolOutputFormat OutputFormat { get; set; } = ToolOutputFormat.JsonCompact;
}
