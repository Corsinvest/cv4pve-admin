/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.UI;

namespace Corsinvest.ProxmoxVE.Admin.Core.Options;

public partial class RenderAdminOptions
{
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    public override async Task SaveAsync()
    {
        foreach (var item in Options.Clusters)
        {
            try
            {
                var client = await PveClientService.GetClient(item);
                if (client != null)
                {
                    //if (!await PveAdminHelper.CheckIsValidVersion(client))
                    //{
                    //    UINotifier.Show(L["{0} - Proxmoxm VE version nont valid! Required {1}", item.FullName, PveAdminHelper.MinimalVersion], UINotifierSeverity.Error);
                    //}
                    var info = await client.GetClusterInfo();
                    item.Name = info.Name;
                    item.Type = info.Type;
                }
            }
            catch (Exception ex)
            {
                UINotifier.Show(ex.Message, UINotifierSeverity.Error);
            }
        }

        await base.SaveAsync();
    }
}