/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Report;

namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Models;

public class JobResult : JobResultBase, IId, IClusterName
{
    public int Id { get; set; }

    [Required] public string ClusterName { get; set; } = default!;

    public Report.Settings Settings { get; set; } = Report.Settings.Fast();

    public ReportFormat Format { get; set; } = ReportFormat.Xlsx;

    public string FileName => Path.Combine(new Module().PathData, $"{Id}.zip");
}
