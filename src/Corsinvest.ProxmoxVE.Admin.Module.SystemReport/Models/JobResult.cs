/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;

namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Models;

public class JobResult : JobResultBase, IId, IClusterName
{
    public int Id { get; set; }

    [Required] public string ClusterName { get; set; } = default!;

    public string NodeNames { get; set; } = "@all";
    public NodeFeature NodeFeatures { get; set; } = NodeFeature.All;

    public string VmIds { get; set; } = "@all";
    public VmFeature VmFeatures { get; set; } = VmFeature.All;

    public string StorageNames { get; set; } = "@all";
    public StorageFeature StorageFeatures { get; set; } = StorageFeature.All;

    public RrdDataTimeFrame RrdDataTimeFrame { get; set; } = RrdDataTimeFrame.Day;
    public RrdDataConsolidation RrdDataConsolidation { get; set; } = RrdDataConsolidation.Average;
    public string FileName => Path.Combine(new Module().PathData, $"{Id}.xlsx");
}
