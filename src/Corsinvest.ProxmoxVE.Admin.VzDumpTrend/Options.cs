/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Validators;
using Corsinvest.ProxmoxVE.Admin.Core.UI.Options;

namespace Corsinvest.ProxmoxVE.Admin.VzDumpTrend;

public class Options : PveModuleClustersOptions<ModuleClusterOptions> { }

public class ModuleClusterOptions : IClusterName
{
    public string ClusterName { get; set; } = default!;

    [Display(Name = "Enabled")]
    public bool Enabled { get; set; }

    [Required]
    [Display(Name = "Cron Schedule")]
    [CronExpressionValidator]
    public string CronExpression { get; set; } = "0 */1 * * *";

    [Display(Name = "Max days logs")]
    public int MaxDaysLogs { get; set; } = 30;
}