/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Module.AIServer.Services;

namespace Corsinvest.ProxmoxVE.Admin.Module.AIServer.Tools;

[McpServerToolType]
internal static class ClusterTools
{
    [McpServerTool, Description("List all configured clusters with their name and description")]
    public static string ListClusters(IAdminService adminService, IAiServerService aiServerService)
    {
        var clusters = adminService.Where(a => a.Settings.Enabled)
                                   .OrderBy(c => c.Settings.Name)
                                   .Select(c => new
                                   {
                                       name = c.Settings.Name,
                                       description = c.Settings.Description
                                   });

        return aiServerService.SerializeTable(clusters);
    }
}
