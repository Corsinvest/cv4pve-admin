/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Corsinvest.ProxmoxVE.Admin.Module.AIServer.Helpers;

public static class McpToolsHelper
{
    public static (ClusterClient? Client, string? ErrorJson) GetClusterClient(
        IHttpContextAccessor httpContextAccessor,
        IAdminService adminService)
    {
        // Read clusterName from HttpContext (set by filter in Module.Map)
        var clusterName = httpContextAccessor.HttpContext?.Items["ClusterName"]?.ToString();

        if (string.IsNullOrWhiteSpace(clusterName))
        {
            return (null, JsonSerializer.Serialize(new { error = "Cluster name not found in context" }));
        }

        // Get cluster client from admin service
        var client = adminService[clusterName];
        if (client == null)
        {
            return (null, JsonSerializer.Serialize(new { error = $"Cluster '{clusterName}' not found" }));
        }

        return (client, null);
    }
}
