/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public class ClusterResourceVmExtraInfo : ClusterResource, IClusterResourceVmOsInfo, INode, IVmId, ISnapshotsSize
{
    [Display(Name = "Snapshots Size")]
    [DisplayFormat(DataFormatString = "{0:" + FormatHelper.FormatBytes + "}")]
    public double SnapshotsSize { get; set; }

    public VmQemuAgentOsInfo VmQemuAgentOsInfo { get; set; } = default!;

    [Display(Name = "Host Name")]
    public string HostName { get; set; } = default!;

    [Display(Name = "Os Version")]
    public string OsVersion { get; set; } = default!;

    [Display(Name = "Os Type")]
    public VmOsType? OsType { get; set; } = default!;
}
