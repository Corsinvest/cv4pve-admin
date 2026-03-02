/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class AdminServiceExtensions
{
    extension(IAdminService adminService)
    {
        public IEnumerable<ClusterClient> GetFrom(IEnumerable<string> clusterNames)
            => [.. adminService.Where(a => a.Settings.Enabled).Where(a => clusterNames.Contains(a.Settings.Name), clusterNames.Any())];
    }
}
