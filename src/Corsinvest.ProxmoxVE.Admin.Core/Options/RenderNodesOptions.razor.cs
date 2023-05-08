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

    private async Task FindNewNodes()
    {
        LoadingFindNewNodes = true;

        try
        {
            var client = await PveClientService.GetClient(ClusterOptions);
            if (client != null)
            {
                var added = false;
                foreach (var (host, ipAddress) in await client.GetHostAndIp())
                {
                    if (ClusterOptions.GetNodeOptions(ipAddress, host) == null)
                    {
                        ClusterOptions.Nodes.Add(new()
                        {
                            IpAddress = ipAddress
                        });
                        added = true;
                    }
                }

                await DataGridManager.Refresh();

                UINotifier.Show(added, L["New nodes added! Please save."], L["No nodes found!"]);
            }
            else
            {
                UINotifier.Show(L["Credential or host not valid!"], UINotifierSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            UINotifier.Show(ex.Message, UINotifierSeverity.Error);
        }

        LoadingFindNewNodes = false;
    }
}
