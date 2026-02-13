/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Models;

internal class AutoSnapInfo : INode, IVmId, ISnapshotsSize, IName, IDescription
{
    public string Node { get; set; } = default!;
    public long VmId { get; set; }
    public string VmName { get; set; } = default!;
    public VmType VmType { get; set; }
    public DateTime Date { get; set; }
    public string Parent { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Label { get; set; } = default!;
    public string Description { get; set; } = default!;
    public bool VmStatus { get; set; }

    [Display(Name = "Snapshots Size")]
    [DisplayFormat(DataFormatString = FormatHelper.DataFormatBytes)]
    public double SnapshotsSize { get; set; }
}
