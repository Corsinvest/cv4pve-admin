/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

public record PermissionData(string ClusterName,
                             string PermissionKey,
                             string Path,
                             bool Propagated = false,
                             bool BuiltIn = false);
