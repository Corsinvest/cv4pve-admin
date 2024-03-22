/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Validators;
using Corsinvest.ProxmoxVE.Admin.Core.Repository;
using Corsinvest.ProxmoxVE.Admin.Core.UI.Options;

namespace Corsinvest.ProxmoxVE.Admin.ClusterUsage;

public class Options : PveModuleClustersOptions<ModuleClusterOptions> { }

public class ModuleClusterOptions : IClusterName
{
    public string ClusterName { get; set; } = default!;

    [Required]
    [Display(Name = "Cron Schedule")]
    [CronExpressionValidator]
    public string CronExpression { get; set; } = "0 1 * * *";

    public bool Enabled { get; set; }

    [Display(Name = "Number of months which should will keep")]
    public int Keep { get; set; } = 12;

    public List<StorageOptions> Storages { get; set; } = [];

    [Display(Name = "Cost Day CPU Running")]
    public double CostDayCpuRunning { get; set; }

    [Display(Name = "Cost Day CPU Stopped")]
    public double CostDayCpuStopped { get; set; }

    [Display(Name = "Cost Day GB Running")]
    public double CostDayMemoryGbRunning { get; set; }

    [Display(Name = "Cost Day GB Stopped")]
    public double CostDayMemoryGbStopped { get; set; }
}