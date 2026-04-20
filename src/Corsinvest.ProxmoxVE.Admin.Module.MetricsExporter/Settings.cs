/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.ComponentModel.DataAnnotations;
using Corsinvest.ProxmoxVE.Admin.Core.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;

namespace Corsinvest.ProxmoxVE.Admin.Module.MetricsExporter;

public class Settings : IModuleSettings
{
    [Required] public string ClusterName { get; set; } = default!;
    public bool Enabled { get; set; }

    public Metrics.Exporter.Api.Settings ApiSettings { get; set; } = Metrics.Exporter.Api.Settings.Fast();

    [Required]
    [Encrypt]
    public string Token { get; set; } = string.Empty;
}
