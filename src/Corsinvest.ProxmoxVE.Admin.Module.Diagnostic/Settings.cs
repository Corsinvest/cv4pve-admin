/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Core.Notifier;
using DiagnosticApiSettings = Corsinvest.ProxmoxVE.Diagnostic.Api.Settings;

namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic;

public class Settings : JobScheduleBase, IModuleSettings, INotifierConfigurationsSettings
{
    public Settings() => CronExpression = "0 6 * * *";
    [Required] public string ClusterName { get; set; } = default!;
    public DiagnosticApiSettings ApiSettings { get; set; } = new();

    public int Keep { get; set; } = 30;

    public IEnumerable<string> NotifierConfigurations { get; set; } = default!;
}
