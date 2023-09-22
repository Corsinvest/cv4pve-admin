/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Extensions;

namespace Corsinvest.ProxmoxVE.Admin.VmUnlock;

public partial class Widget
{
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    private int Count { get; set; } = -1;
    private string? Locks { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var data = await Helper.GetVmLocks(await PveClientService.GetClientCurrentClusterAsync());
        Count = data.Count();
        Locks = data.Select(a => a.Lock).Distinct().JoinAsString(",");
    }
}