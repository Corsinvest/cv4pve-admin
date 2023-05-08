/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Entities;
using Corsinvest.ProxmoxVE.Admin.Core.Repository;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.ClusterUsage.Models;

public class DataVm : EntityBase<int>, IClusterName
{
    [Required]
    public string ClusterName { get; set; } = default!;

    public long VmId { get; set; }
    public string VmName { get; set; } = default!;
    public string Node { get; set; } = default!;
    public int CpuSize { get; set; }


    [Display(Name = "CPU Usage %")]
    [DisplayFormat(DataFormatString = "{0:P1}")]
    public double CpuUsagePercentage { get; set; }

    [Display(Name = "Max Memory")]
    [DisplayFormat(DataFormatString = "{0:" + FormatHelper.FormatBytes + "}")]
    public long MemorySize { get; set; }

    [Display(Name = "Memory Usage")]
    [DisplayFormat(DataFormatString = "{0:" + FormatHelper.FormatBytes + "}")]
    public long MemoryUsage { get; set; }

    [Display(Name = "Memory Usage %")]
    [DisplayFormat(DataFormatString = "{0:P1}")]
    public double MemoryUsagePercentage => MemoryUsage / MemorySize;

    public DateTime Date { get; set; } = default!;
    public List<DataVmStorage> Storages { get; set; } = new();
}