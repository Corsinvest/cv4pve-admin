/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.Layout;

public partial class WidgetInfoBase
{
    [Parameter] public string Description { get; set; } = default!;
    [Parameter] public Func<PveClient, string, Task<bool>> GetStatus { get; set; } = default!;

    [Inject] private IPveClientService PveClientService { get; set; } = default!;
    private bool? Status { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (GetStatus != null)
        {
            Status = await GetStatus(await PveClientService.GetClientCurrentClusterAsync(), await PveClientService.GetCurrentClusterNameAsync());
        }
    }
}