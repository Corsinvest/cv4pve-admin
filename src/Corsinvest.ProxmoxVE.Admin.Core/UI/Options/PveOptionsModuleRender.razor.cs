/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Repository;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.Options;

public partial class PveOptionsModuleRender<T> where T : IClusterName
{
    [Parameter] public PveModuleClustersOptions<T> Options { get; set; } = default!;
    [Parameter] public RenderFragment<T> Row { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;
}