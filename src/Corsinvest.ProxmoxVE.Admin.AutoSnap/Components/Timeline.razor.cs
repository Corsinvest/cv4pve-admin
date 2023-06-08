/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Ardalis.Specification;
using Corsinvest.AppHero.Core.Domain.Repository;
using Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Repository;

namespace Corsinvest.ProxmoxVE.Admin.AutoSnap.Components;

public partial class Timeline
{
    [Inject] public IReadRepository<AutoSnapJobHistory> History { get; set; } = default!;
    [Inject] public IPveClientService PveClientService { get; set; } = default!;

    private IEnumerable<IGrouping<DateTime, AutoSnapJobHistory>> Data { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var spec = new AutoSnapJobHistorySpec(await PveClientService.GetCurrentClusterName());
        Data = (await History.ListAsync(spec)).GroupBy(a => a.Start.Date).Take(10);
    }
}
