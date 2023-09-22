/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Options;
using Corsinvest.AppHero.Core.UI;
using Corsinvest.ProxmoxVE.Admin.Core.Options;

namespace Corsinvest.ProxmoxVE.Admin.Core.Support.Subscription;

public partial class RenderIndex
{
    [Inject] private IOptionsSnapshot<AdminOptions> AdminOptions { get; set; } = default!;
    [Inject] private IWritableOptionsService<AdminOptions> WritableOptionsService { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    private Dictionary<ClusterNodeOptions, Info> Checks { get; } = new();
    private bool Initialized { get; set; }
    private bool InSave { get; set; }
    private bool InRetrieveNodeInfo { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Initialized = false;
        Checks.AddRange(await Helper.CheckAsync(AdminOptions.Value));
        StateHasChanged();
        Initialized = true;
    }

    private async Task RetrieveNodeInfo(ClusterOptions clusterOptions)
    {
        InRetrieveNodeInfo = true;
        try
        {
            var ret = await PveClientService.PopulateInfoNodesAsync(clusterOptions);

            switch (ret)
            {
                case -1:
                    UINotifier.Show(L["Credential or host not valid!"], UINotifierSeverity.Error);
                    break;

                case 0:
                    UINotifier.Show(L["All nodes have been inserted and updated!"], UINotifierSeverity.Info);
                    break;

                case 1:
                    UINotifier.Show(L["New nodes added and updated! Saved."], UINotifierSeverity.Info);
                    WritableOptionsService.Update(AdminOptions.Value);
                    break;

                default: break;
            }
        }
        catch (Exception ex) { UINotifier.Show(ex.Message, UINotifierSeverity.Error); }
        InRetrieveNodeInfo = false;
    }

    private async Task Save(ClusterNodeOptions nodeOptions)
    {
        InSave = true;
        WritableOptionsService.Update(AdminOptions.Value);
        Checks[nodeOptions] = await Helper.RegisterAsync(nodeOptions.ServerId, nodeOptions.SubscriptionId);
        InSave = false;
        StateHasChanged();

        UINotifier.Show(L["Data of node has been saved!"], UINotifierSeverity.Info);
    }
}