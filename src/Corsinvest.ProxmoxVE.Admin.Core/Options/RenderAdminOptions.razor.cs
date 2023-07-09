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
                await PveClientService.PopulateInfoNodes(item);
            }
            catch (Exception ex) { UINotifier.Show(ex.Message, UINotifierSeverity.Error); }
        }

        await base.SaveAsync();
    }
}