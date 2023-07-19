/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Options;
using Corsinvest.AppHero.Core.UI;
using Corsinvest.ProxmoxVE.Admin.Core.Options;

namespace Corsinvest.ProxmoxVE.Admin.Core.Subscription;

public partial class RenderIndex
{
    [Inject] private IOptionsSnapshot<AdminOptions> AdminOptions { get; set; } = default!;
    [Inject] private IWritableOptionsService<AdminOptions> WritableOptionsService { get; set; } = default!;
    [Inject] private SubscriptionService SubscriptionService { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    public Dictionary<ClusterNodeOptions, Info> Checks { get; } = new();
    private bool Initialized { get; set; }
    private bool InSave { get; set; }
    private bool InRetrieveNodeInfo { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Initialized = false;

        foreach (var cluster in AdminOptions.Value.Clusters)
        {
            foreach (var node in cluster.Nodes)
            {
                Checks.Add(node,
                           string.IsNullOrWhiteSpace(node.ServerId) || string.IsNullOrWhiteSpace(node.SubscriptionId)
                                ? new Info() { Status = Status.Invalid }
                                : await SaveOnline(node));
            }
        }

        Initialized = true;
    }

    private async Task<Info> SaveOnline(ClusterNodeOptions nodeOptions)
        => await SubscriptionService.RegisterAsync(nodeOptions.ServerId, nodeOptions.SubscriptionId);

    private async Task RetrieveNodeInfo(ClusterOptions clusterOptions)
    {
        try
        {
            InRetrieveNodeInfo = true;
            var ret = await PveClientService.PopulateInfoNodes(clusterOptions);
            InRetrieveNodeInfo = false;

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
    }

    private async Task Save(ClusterNodeOptions nodeOptions)
    {
        InSave = true;
        WritableOptionsService.Update(AdminOptions.Value);
        await SaveOnline(nodeOptions);
        InSave = false;

        UINotifier.Show(L["Data of node has been saved!"], UINotifierSeverity.Info);
    }
}