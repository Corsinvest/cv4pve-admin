/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class PermissionHelper
{
    public static string GetPathVm(long vmId) => $"/vms/{vmId}";
    public static string GetPathNode(string node) => $"/nodes/{node}";
    public static string GetPathStorage(string storage) => $"/storages/{storage}";
}
