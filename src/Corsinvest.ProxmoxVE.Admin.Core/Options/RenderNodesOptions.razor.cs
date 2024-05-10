/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.UI;

namespace Corsinvest.ProxmoxVE.Admin.Core.Options;

public partial class RenderNodesOptions
{
    [EditorRequired][Parameter] public ClusterOptions ClusterOptions { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;
    [Inject] private IDataGridManager<ClusterNodeOptions> DataGridManager { get; set; } = default!;

    private bool LoadingFindNewNodes { get; set; }

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Nodes"];
        DataGridManager.DefaultSort = new() { [nameof(ClusterNodeOptions.IpAddress)] = false };
        DataGridManager.QueryAsync = async () => await Task.FromResult(ClusterOptions.Nodes);

        DataGridManager.SaveAsync = async (item, isNew) =>
        {
            if (isNew) { ClusterOptions.Nodes.Add(item); }
            return await Task.FromResult(true);
        };

        DataGridManager.DeleteAsync = async (items) =>
        {
            foreach (var item in items) { ClusterOptions.Nodes.Remove(item); }
            return await Task.FromResult(true);
        };
    }

    private async Task FindNewNodesAsync()
    {
        LoadingFindNewNodes = true;

        try
        {
            switch (await PveClientService.PopulateInfoNodesAsync(ClusterOptions))
            {
                case -1:
                    UINotifier.Show(L["Credential or host not valid!"], UINotifierSeverity.Error);
                    break;

                case 0:
                    UINotifier.Show(L["All nodes have been inserted and updated!"], UINotifierSeverity.Info);
                    break;

                case 1:
                    await DataGridManager.RefreshAsync();
                    UINotifier.Show(L["New nodes added and updated! Please save."], UINotifierSeverity.Info);
                    break;

                default: break;
            }
        }
        catch (Exception ex) { UINotifier.Show(ex.Message, UINotifierSeverity.Error); }

        LoadingFindNewNodes = false;
    }
}
