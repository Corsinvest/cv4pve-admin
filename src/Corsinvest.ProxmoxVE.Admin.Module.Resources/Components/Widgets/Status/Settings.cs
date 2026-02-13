/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components.Widgets.Status;

public class Settings
{
    public VmType VmType { get; set; } = VmType.Qemu;
    public ClusterResourceType ResourceType { get; set; } = ClusterResourceType.Vm;
}
