/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json;
using Corsinvest.ProxmoxVE.Admin.Module.AIServer.Helpers;

namespace Corsinvest.ProxmoxVE.Admin.Module.AIServer.Services;

internal class AiServerService(IPermissionService permissionService,
                               IAdminService adminService,
                               ISettingsService settingsService) : IAiServerService
{
    public string SerializeTable<T>(IEnumerable<T> items)
        => ToolHelper.SerializeTable(items, settingsService.GetForModule<Module, Settings>(ApplicationHelper.AllClusterName).OutputFormat);

    public async Task<bool> CanExecuteToolAsync(string clusterName, Permission permission)
        => await permissionService.HasAsync(clusterName, permission);

    public (ClusterClient? Client, string? ErrorJson) GetClusterClient(string clusterName)
    {
        if (string.IsNullOrWhiteSpace(clusterName))
        {
            return (null, JsonSerializer.Serialize(new { error = "Cluster name is required" }));
        }

        var client = adminService[clusterName];
        return client == null
                ? (null, JsonSerializer.Serialize(new { error = $"Cluster '{clusterName}' not found" }))
                : (client, null);
    }

    public async Task<IEnumerable<ClusterResource>> HasAsync(string clusterName, IEnumerable<ClusterResource> items)
        => await permissionService.FilterAsync(clusterName, items);
}
