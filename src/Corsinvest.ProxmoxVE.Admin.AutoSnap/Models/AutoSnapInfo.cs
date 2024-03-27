/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Models;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;
using System.ComponentModel;

namespace Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;

internal class AutoSnapInfo : INode, IVmId, ISnapshotsSize, IName
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
    [DisplayFormat(DataFormatString = "{0:" + FormatHelper.FormatBytes + "}")]
    public double SnapshotsSize { get; set; }

    [DisplayName("Size")]
    public string TextSize => FormatHelper.FromBytes(SnapshotsSize);
}