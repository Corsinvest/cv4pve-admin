/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Entities;

namespace Corsinvest.ProxmoxVE.Admin.Diagnostic.Models;

public class ExecutionData : EntityBase<int>
{
    public int ExecutionId { get; set; }

    [Required]
    public Execution Execution { get; set; } = default!;

    public string Data { get; set; } = default!;
}
