/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
using Microsoft.AspNetCore.Http;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE;

/// <summary>
/// Endpoint filter that verifies the current user has access to the cluster
/// specified in the route parameter <c>{clusterName}</c> before the handler runs.
/// Checks the app-level <see cref="ClusterPermissions.Cluster.SelectCluster"/> permission.
/// Specific resource checks (Vm/Node/Storage) remain responsibility of the handler.
/// </summary>
public class ClusterAccessEndpointFilter(IPermissionService permissionService) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var clusterName = context.HttpContext.Request.RouteValues["clusterName"]?.ToString();
        if (string.IsNullOrEmpty(clusterName)) { return await next(context); }

        if (!await permissionService.HasAsync(clusterName,
                                              ClusterPermissions.Cluster.SelectCluster.Key,
                                              "*"))
        {
            return Results.Unauthorized();
        }

        return await next(context);
    }
}
