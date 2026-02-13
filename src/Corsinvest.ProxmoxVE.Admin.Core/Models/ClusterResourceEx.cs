/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public class ClusterResourceEx : ClusterResource, IClusterName, ISnapshotsSize, ISnapshotsReplicationSize, IClusterResourceVmOsInfo
{
    public const string CommandsColumnName = "Cmds";
    public const string UsageCpuMemDiskPercColumnName = "UsageCpuMemDiskPercColumnName";
    public const string OsIcon = "OsIcon";

    public string ClusterName { get; set; } = default!;
    public string Link { get; set; } = default!;
    public double SnapshotsSize { get; set; }
    public double SnapshotsReplicationSize { get; set; }

    #region VmInfo
    public VmQemuAgentOsInfo VmQemuAgentOsInfo { get; set; } = default!;
    public string HostName { get; set; } = default!;
    public string OsVersion { get; set; } = default!;
    public VmOsType? OsType { get; set; }
    #endregion

    public void Set(IClusterResourceVmOsInfo info)
    {
        VmQemuAgentOsInfo = info.VmQemuAgentOsInfo;
        HostName = info.HostName;
        OsVersion = info.OsVersion;
        OsType = info.OsType;
    }
}
