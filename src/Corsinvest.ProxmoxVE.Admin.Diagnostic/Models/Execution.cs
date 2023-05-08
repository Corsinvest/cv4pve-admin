/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Entities;

namespace Corsinvest.ProxmoxVE.Admin.Diagnostic.Models;

public class Execution : EntityBase<int>, IClusterName
{
    [Required]
    public string ClusterName { get; set; } = default!;

    public int Warning { get; set; }
    public int Critical { get; set; }
    public int Info { get; set; }
    public DateTime Date { get; set; }

    [Required]
    public ExecutionData Data { get; set; } = default!;
}
