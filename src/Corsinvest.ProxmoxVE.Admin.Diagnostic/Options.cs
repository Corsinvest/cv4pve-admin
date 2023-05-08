/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Notification;
using Corsinvest.AppHero.Core.Validators;
using Corsinvest.ProxmoxVE.Admin.Core.UI.Options;
using Newtonsoft.Json;
using DiagnosticApiOptions = Corsinvest.ProxmoxVE.Diagnostic.Api.Settings;

namespace Corsinvest.ProxmoxVE.Admin.Diagnostic;

public class Options : PveModuleClustersOptions<ModuleClusterOptions> { }

public class ModuleClusterOptions : DiagnosticApiOptions, IClusterName, INotificationChannelsOptions
{
    public string ClusterName { get; set; } = default!;

    [Required]
    [Display(Name = "Cron Schedule")]
    [CronExpressionValidator]
    public string CronExpression { get; set; } = default!;

    [Display(Name = "Enabled")]
    public bool Enabled { get; set; }

    [Display(Name = "Number which should will keep")]
    public int Keep { get; set; } = 30;

    public IEnumerable<string> NotificationChannels { get; set; } = default!;
}
