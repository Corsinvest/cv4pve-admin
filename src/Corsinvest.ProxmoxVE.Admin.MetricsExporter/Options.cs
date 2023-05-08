/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Repository;
using Corsinvest.ProxmoxVE.Admin.Core.UI.Options;
using System.ComponentModel.DataAnnotations;

namespace Corsinvest.ProxmoxVE.Admin.MetricsExporter;

public class Options : PveModuleClustersOptions<ModuleClusterOptions> { }

public class ModuleClusterOptions : IClusterName
{
    public string ClusterName { get; set; } = default!;

    [Display(Name = "Prometheus Exporter Node Disk Info (require more time)")]
    public bool PrometheusExporterNodeDiskInfo { get; set; }

    [Display(Name = "Prometheus Exporter Prefix")]
    public string PrometheusExporterPrefix { get; set; } = "cv4pve";
}
