/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Vm;

public partial class Charts
{
    [Parameter] public Func<RrdDataTimeFrame, RrdDataConsolidation, Task<IEnumerable<VmRrdData>>> GetItems { get; set; } = default!;

    private RrdDataTimeFrame RrdDataTimeFrame { get; set; } = RrdDataTimeFrame.Day;
    private RrdDataConsolidation RrdDataConsolidation { get; set; } = RrdDataConsolidation.Average;
    private IEnumerable<VmRrdData> Items { get; set; } = default!;

    protected override async Task OnInitializedAsync() => await Refresh();

    public async Task Refresh()
    {
        Items = null!;
        Items = await GetItems(RrdDataTimeFrame, RrdDataConsolidation);
    }
}
