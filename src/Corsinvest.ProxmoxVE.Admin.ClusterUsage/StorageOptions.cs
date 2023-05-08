/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.ClusterUsage;

public class StorageOptions
{
    public string Storage { get; set; } = default!;

    [Display(Name = "Cost Day GB Running")]
    public double CostDayGbRunning { get; set; }

    [Display(Name = "Cost Day GB Stopped")]
    public double CostDayGbStopped { get; set; }
}