/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

namespace Corsinvest.ProxmoxVE.Admin.Core.Commands;

public interface ICommand<TResult>
{
    string ClusterName { get; }
    Permission RequiredPermission { get; }
    string Context { get; }
    Task<bool> HasPermissionAsync(IPermissionService permissionService);
}
