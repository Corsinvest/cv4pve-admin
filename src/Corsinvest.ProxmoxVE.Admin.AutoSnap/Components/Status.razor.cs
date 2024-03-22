/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.BaseUI.DataManager;
using Corsinvest.AppHero.Core.Domain.Repository;
using Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;
using Microsoft.Extensions.Logging;

namespace Corsinvest.ProxmoxVE.Admin.AutoSnap.Components;

public partial class Status
{
    [Parameter] public string Height { get; set; } = default!;
    [Parameter] public string VmIdsOrNames { get; set; } = default!;

    [Inject] private IDataGridManager<AutoSnapInfo> DataGridManager { get; set; } = default!;
    [Inject] private IJobService JobService { get; set; } = default!;
    [Inject] private IOptionsSnapshot<Options> Options { get; set; } = default!;
    [Inject] private ILoggerFactory LoggerFactory { get; set; } = default!;
    [Inject] private IReadRepository<AutoSnapJob> Jobs { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    private readonly AggregateDefinition<AutoSnapInfo> SizeAggregation = new()
    {
        Type = AggregateType.Custom,
        CustomAggregate = x => $"Total: {FormatHelper.FromBytes(x.Sum(a => a.Size))}"
    };

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Status"];
        DataGridManager.DefaultSort = new()
        {
            [nameof(AutoSnapInfo.Label)] = false,
            [nameof(AutoSnapInfo.VmId)] = false,
            [nameof(AutoSnapInfo.Parent)] = true
        };

        DataGridManager.QueryAsync = async () =>
        {
            var clusterName = await PveClientService.GetCurrentClusterNameAsync();

            var options = Options.Value.Get(clusterName);
            var vmIdsOrNames = VmIdsOrNames;
            if (string.IsNullOrWhiteSpace(VmIdsOrNames))
            {
                vmIdsOrNames = options.SearchMode == SearchMode.Managed
                                    ? await Helper.GetVmIdsOrNames(Jobs, clusterName, false)
                                    : Helper.AllVms;
            }

            var client = (await PveClientService.GetClientCurrentClusterAsync())!;
            var data = await Helper.GetInfo(client, options, LoggerFactory, vmIdsOrNames);

            //snapshot size
            var disks = await PveClientService.GetDisksInfo(client, (await PveClientService.GetCurrentClusterOptionsAsync())!);

            foreach (var item in data)
            {
                item.Size = disks.Where(a => a.VmId == item.VmId)
                                 .SelectMany(a => a.Snapshots)
                                 .Where(a => a.Name == item.Name && !a.Replication)
                                 .Select(a => a.Size)
                                 .DefaultIfEmpty(0)
                                 .Sum();
            }

            return data;
        };
    }

    private async Task Delete()
    {
        if (await UIMessageBox.ShowQuestionAsync(L["Delete AutoSnap?"], "Delete AutoSnap"))
        {
            var clusterName = await PveClientService.GetCurrentClusterNameAsync();
            JobService.Schedule<Job>(a => a.Delete(DataGridManager.SelectedItems, clusterName), TimeSpan.FromSeconds(10));
            UINotifier.Show(L["Deleting snapshots!"], UINotifierSeverity.Info);
            DataGridManager.SelectedItems.Clear();
        }
    }
}