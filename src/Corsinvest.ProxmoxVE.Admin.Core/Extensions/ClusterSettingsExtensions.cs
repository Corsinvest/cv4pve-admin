/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class ClusterSettingsExtensions
{
    extension(ClusterSettings s)
    {
        public bool SshIsConfigured => s.SshCredential.IsConfigured;

        public string TypeLabel
            => s.Type switch
            {
                ClusterType.Cluster => "CLUSTER",
                ClusterType.SingleNode => "NODE",
                _ => "NODE"
            };

        public string TypeIcon
            => s.Type switch
            {
                ClusterType.Cluster => PveAdminUIHelper.Icons.Cluster,
                ClusterType.SingleNode => PveAdminUIHelper.Icons.Node,
                _ => PveAdminUIHelper.Icons.Node
            };

        public string DisplayName
        {
            get
            {
                var ret = s.Name == s.PveName
                        ? s.Name
                        : $"{s.Name} ({s.PveName})";

                return $"{s.TypeLabel}: {ret}";
            }
        }

        public string FullDisplayName
        {
            get
            {
                var fullName = s.DisplayName;
                if (!string.IsNullOrEmpty(s.Description)) { fullName = $"{fullName} - {s.Description}"; }

                return fullName;
            }
        }

        public string ApiHostsAndPortHA
            => s.Nodes.Select(a => $"{a.IPAddress}:{a.ApiPort}").JoinAsString(",");

        public ClusterNodeSettings? GetNodeSettings(string iPAddress, string host)
            => s.Nodes.FirstOrDefault(a => a.IPAddress == iPAddress
                                            || a.IPAddress.Equals(host, StringComparison.CurrentCultureIgnoreCase));
    }
}
