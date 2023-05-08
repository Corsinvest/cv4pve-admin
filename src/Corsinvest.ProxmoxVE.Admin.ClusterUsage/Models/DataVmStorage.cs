/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Entities;

namespace Corsinvest.ProxmoxVE.Admin.ClusterUsage.Models;

public class DataVmStorage : EntityBase<int>
{
    public DataVm DataVm { get; set; } = default!;
    public string Storage { get; set; } = default!;
    public long Size { get; set; }
}
