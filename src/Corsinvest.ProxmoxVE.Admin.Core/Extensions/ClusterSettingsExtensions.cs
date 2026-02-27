/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class ClusterSettingsExtensions
{
    extension(ClusterSettings s)
    {
        public ClusterNodeSettings? GetNodeSettings(string iPAddress, string host)
            => s.Nodes.FirstOrDefault(a => a.IPAddress == iPAddress
                                            || a.IPAddress.Equals(host, StringComparison.CurrentCultureIgnoreCase));

    }
}
