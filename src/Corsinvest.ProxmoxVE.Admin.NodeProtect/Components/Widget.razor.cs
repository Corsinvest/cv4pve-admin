/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Repository;
using Corsinvest.ProxmoxVE.Admin.Core.Repository;
using Corsinvest.ProxmoxVE.Admin.Core.Services;

namespace Corsinvest.ProxmoxVE.Admin.NodeProtect.Components;

public partial class Widget
{
    [Inject] private IReadRepository<NodeProtectJobHistory> JobHistories { get; set; } = default!;
    [Inject] private IOptionsSnapshot<Options> Options { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;
    private string Last { get; set; } = default!;
    private bool Enabled { get; set; }
    private int Count { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var clusterName = await PveClientService.GetCurrentClusterName();

        Enabled = Options.Value.Get(clusterName).Enabled;
        var spec = new ClusterByNameSpec<NodeProtectJobHistory>(clusterName);
        Count = await JobHistories.CountAsync(spec);

        var item = (await JobHistories.ListAsync(spec)).OrderByDescending(a => a.Start).FirstOrDefault();
        Last = item == null
                ? "-"
                : item.Start.ToString();
    }
}