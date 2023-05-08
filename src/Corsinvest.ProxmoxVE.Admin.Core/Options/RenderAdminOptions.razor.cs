/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Options;

public partial class RenderAdminOptions
{
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    public override async Task SaveAsync()
    {
        foreach (var item in Options.Clusters)
        {
            var client = await PveClientService.GetClient(item);
            if (client != null) { item.Name = await client.GetClusterName(); }
        }

        await base.SaveAsync();
    }
}