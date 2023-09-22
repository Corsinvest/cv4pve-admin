/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Extensions;
using Corsinvest.AppHero.Core.MudBlazorUI.Extensions;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;

namespace Corsinvest.ProxmoxVE.Admin.DisksStatus.Components;

public partial class DisksList
{
    [Parameter] public string Node { get; set; } = default!;
    [Parameter] public PveClient PveClient { get; set; } = default!;

    [Inject] private IDataGridManager<NodeDiskList> DataGridManager { get; set; } = default!;
    [Inject] private IPveUtilityService PveUtilityService { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    private bool BlinkLedLoading { get; set; }

    private readonly List<string> BlinkStatus = new();

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Disks"];
        DataGridManager.DefaultSort = new() { [nameof(NodeDiskList.DevPath)] = false };
        DataGridManager.QueryAsync = async () => await PveClient.Nodes[Node].Disks.List.Get();
    }

    private async Task BlinkLed(string node)
    {
        BlinkLedLoading = true;
        if (await UIMessageBox.ShowQuestionAsync(L["Blink/Unblink"], L["For disks?"]))
        {
            var ret = new List<FluentResults.Result>();

            foreach (var item in DataGridManager.ToDataGridManager().DataGrid!.SelectedItems)
            {
                var blink = BlinkStatus.Contains(item.DevPath);
                if (blink)
                {
                    BlinkStatus.Remove(item.DevPath);
                }
                else
                {
                    BlinkStatus.Add(item.DevPath);
                }
                blink = !blink;

                ret.Add(await PveUtilityService.BlinkDiskLedAsync(await PveClientService.GetCurrentClusterNameAsync(), node, item.DevPath, blink));
            }

            UINotifier.Show(!ret.Any(a => a.IsFailed),
                            L["Blink successfully!"],
                            L["Error execution!<br>"] + ret.SelectMany(a => a.Errors.Select(a => a.Message)).JoinAsString("<br>"));
        }
        BlinkLedLoading = false;
    }

    private string RowClassFunc(NodeDiskList item, int rowNumber)
        => BlinkStatus.Contains(item.DevPath)
            ? "blink"
            : string.Empty;
}