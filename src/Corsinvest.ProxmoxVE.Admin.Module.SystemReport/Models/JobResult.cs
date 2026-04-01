/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Models;

public class JobResult : JobResultBase, IId, IClusterName
{
    public int Id { get; set; }

    [Required] public string ClusterName { get; set; } = default!;

    public Report.Settings Settings { get; set; } = new();

    public string FileName => Path.Combine(new Module().PathData, $"{Id}.xlsx");
}
