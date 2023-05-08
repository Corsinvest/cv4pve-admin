/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;

internal class AutoSnapInfo
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
}