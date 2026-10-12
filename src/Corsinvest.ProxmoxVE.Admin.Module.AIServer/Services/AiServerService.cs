/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json;
using Corsinvest.ProxmoxVE.Admin.Module.AIServer.Helpers;

namespace Corsinvest.ProxmoxVE.Admin.Module.AIServer.Services;

internal class AiServerService(IPermissionService permissionService,
                               IAdminService adminService,
                               ISettingsService settingsService,
                               IModuleService moduleService) : IAiServerService
{
    public string SerializeTable<T>(IEnumerable<T> items)
        => ToolHelper.SerializeTable(items, settingsService.GetForModule<Module, Settings>(ApplicationHelper.AllClusterName).OutputFormat);

    public Task<bool> CanExecuteToolAsync(string clusterName, Permission permission)
        => permissionService.HasAsync(clusterName, permission);

    // A cluster is listed for who can run at least one tool on it
    public async Task<bool> CanSeeClusterAsync(string clusterName)
    {
        foreach (var permission in moduleService.Get<Module>()!.ToolPermissions)
        {
            if (await permissionService.HasAsync(clusterName, permission)) { return true; }
        }

        return false;
    }

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

    public Task<IEnumerable<ClusterResource>> HasAsync(string clusterName, IEnumerable<ClusterResource> items)
        => permissionService.FilterAsync(clusterName, items);
}
