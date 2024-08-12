/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Validators;
using Corsinvest.ProxmoxVE.Admin.Core.Repository;
using Corsinvest.ProxmoxVE.Admin.Core.UI.Options;
using Newtonsoft.Json;

namespace Corsinvest.ProxmoxVE.Admin.NodeProtect;

public class Options : PveModuleClustersOptions<ModuleClusterOptions> { }

public class ModuleClusterOptions : IClusterName
{
    public string ClusterName { get; set; } = default!;

    [Required]
    [Display(Name = "Cron Schedule")]
    [CronExpressionValidator]
    public string CronExpression { get; set; } = default!;

    [Display(Name = "Enabled")]
    public bool Enabled { get; set; }

    [Display(Name = "Directory or file to backup")]
    public string PathsToBackup { get; set; } = @"/etc/.
/var/lib/pve-cluster/.
/var/lib/ceph/.";

    [Display(Name = "Number which should will keep")]
    public int Keep { get; set; } = 30;

    public IEnumerable<string> NotificationChannels { get; set; } = default!;
}
