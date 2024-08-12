/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Repository;
using Corsinvest.ProxmoxVE.Admin.Core.UI.Options;

namespace Corsinvest.ProxmoxVE.Admin.ClusterStatus;

public class Options : PveModuleClustersOptions<ModuleClusterOptions> { }

public class ModuleClusterOptions : IClusterName
{
    public string ClusterName { get; set; } = default!;

    public ThresholdPercentual Cpu { get; set; } = new ThresholdPercentual();
    public ThresholdPercentual Memory { get; set; } = new ThresholdPercentual();
    public ThresholdPercentual Storage { get; set; } = new ThresholdPercentual();

    //    [Range(0, 100)]
    //    [Display(Name = "Max Storage Usage (default: 85%)")]
    //    public int MaxStorageUsage { get; set; } = 85;

    //    [Range(0, 100)]
    //    [Display(Name = "Days Previous (default: 2)")]
    //    public int DaysPrevious { get; set; } = 2;
}
