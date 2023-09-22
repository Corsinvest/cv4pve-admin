/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Services;
using Corsinvest.ProxmoxVE.Admin.ReplicationTrend.Repository;

namespace Corsinvest.ProxmoxVE.Admin.ReplicationTrend.Components;

public partial class RenderWidget
{
    [Inject] private IReadRepository<ReplicationResult> ReplicationResults { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    private string Last { get; set; } = default!;
    private int CountOk { get; set; }
    private int CountKo { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var clusterName = await PveClientService.GetCurrentClusterNameAsync();

        var date = DateTime.Now.AddDays(-1);
        CountOk = await ReplicationResults.CountAsync(new ReplicationResultSpec(clusterName, true, date));
        CountKo = await ReplicationResults.CountAsync(new ReplicationResultSpec(clusterName, false, date));
        var item = await ReplicationResults.FirstOrDefaultAsync(new ReplicationResultSpec(clusterName).OrderDescStart());
        if (item != null) { Last = item.Start.ToString()!; }
    }
}