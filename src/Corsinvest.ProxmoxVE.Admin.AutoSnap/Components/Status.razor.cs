﻿/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.BaseUI.DataManager;
using Corsinvest.AppHero.Core.Domain.Repository;
using Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;
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
            var clusterName = await PveClientService.GetCurrentClusterName();

            var options = Options.Value.Get(clusterName);
            var vmIdsOrNames = VmIdsOrNames;
            if (string.IsNullOrWhiteSpace(VmIdsOrNames))
            {
                vmIdsOrNames = options.SearchMode == SearchMode.Managed
                                    ? await Helper.GetVmIdsOrNames(Jobs, clusterName, false)
                                    : Helper.AllVms;
            }

            return await Helper.GetInfo((await PveClientService.GetClientCurrentCluster())!, options, LoggerFactory, vmIdsOrNames);
        };
    }

    private async Task Delete()
    {
        if (await UIMessageBox.ShowQuestionAsync(L["Delete AutoSnap?"], "Delete AutoSnap"))
        {
            var clusterName = await PveClientService.GetCurrentClusterName();
            JobService.Schedule<Job>(a => a.Delete(DataGridManager.SelectedItems, clusterName), TimeSpan.FromSeconds(10));
            UINotifier.Show(L["Deleting snapshots!"], UINotifierSeverity.Info);
            DataGridManager.SelectedItems.Clear();
        }
    }
}