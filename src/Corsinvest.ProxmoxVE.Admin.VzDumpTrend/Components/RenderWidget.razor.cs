/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Repository;
using Corsinvest.ProxmoxVE.Admin.Core.Services;
using Corsinvest.ProxmoxVE.Admin.VzDumpTrend.Repository;

namespace Corsinvest.ProxmoxVE.Admin.VzDumpTrend.Components;

public partial class RenderWidget
{
    [Inject] private IReadRepository<VzDumpDetail> VzDumpDetails { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    private string Last { get; set; } = default!;
    private int CountOk { get; set; }
    private int CountKo { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var clusterName = await PveClientService.GetCurrentClusterNameAsync();

        var date = DateTime.Now.AddDays(-30);
        CountOk = await VzDumpDetails.CountAsync(new VzDumpDetailSpec(clusterName, true, date));
        CountKo = await VzDumpDetails.CountAsync(new VzDumpDetailSpec(clusterName, false, date));
        var item = await VzDumpDetails.FirstOrDefaultAsync(new VzDumpDetailSpec(clusterName).OrderDescStart());
        if (item != null) { Last = item.Start.ToString()!; }
    }
}