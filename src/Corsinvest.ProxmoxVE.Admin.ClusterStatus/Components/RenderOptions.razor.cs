/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;

namespace Corsinvest.ProxmoxVE.Admin.ClusterStatus.Components;

public partial class RenderOptions
{
    private string[] ThresholdTexts { get; } = new[] { "CPU", "Memory", "Storage" };
    private string[] ThresholdIcons { get; } = new[] { PveBlazorHelper.Icons.Cpu, PveBlazorHelper.Icons.Memory, PveBlazorHelper.Icons.Storage };
}
