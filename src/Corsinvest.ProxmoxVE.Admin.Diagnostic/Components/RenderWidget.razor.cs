/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Services;
using Corsinvest.ProxmoxVE.Admin.Diagnostic.Repository;

namespace Corsinvest.ProxmoxVE.Admin.Diagnostic.Components;

public partial class RenderWidget
{
    [Inject] private IReadRepository<Execution> Execution { get; set; } = default!;
    [Inject] private IOptionsSnapshot<Options> Options { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    private Execution? Last { get; set; }
    private ModuleClusterOptions ModuleClusterOptions { get; set; } = new();
    private int Count { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var clusterName = await PveClientService.GetCurrentClusterName();
        ModuleClusterOptions = Options.Value.Get(clusterName);
        Count = await Execution.CountAsync(new ExecutionSpec(clusterName));
        Last = await Execution.FirstOrDefaultAsync(new ExecutionSpec(clusterName).Last());
    }
}