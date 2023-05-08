/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.BaseUI.DataManager;
using Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Extensions;
using Corsinvest.ProxmoxVE.Admin.Core.Repository;
using Corsinvest.ProxmoxVE.Api;

namespace Corsinvest.ProxmoxVE.Admin.AutoSnap.Components;

public partial class Jobs
{
    [Parameter] public string Height { get; set; } = default!;

    [Inject] private IDataGridManagerRepository<AutoSnapJob> DataGridManager { get; set; } = default!;
    [Inject] private IJobService JobService { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    private PveClient _pveClient = default!;
    private List<string> VmIdsList { get; set; } = default!;

    protected async Task<PveClient> GetPveClient() => _pveClient ??= await PveClientService.GetClientCurrentCluster();

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Jobs "];
        DataGridManager.DefaultSort = new() { [nameof(AutoSnapJob.Id)] = false };

        DataGridManager.QueryAsync = async () =>
        {
            var clusterName = await PveClientService.GetCurrentClusterName();
            return await DataGridManager.Repository.ListAsync(new AutoSnapJobSpec(clusterName));
        };

        DataGridManager.SaveAfterAsync = async (item, isNew) =>
        {
            await Task.CompletedTask;
            JobService.ScheduleOrRemove<Job>(a => a.Create(item.Id), item.CronExpression, item.Enabled, item.ClusterName, item.Id);
        };

        DataGridManager.DeleteAfterAsync = async (items) =>
        {
            await Task.CompletedTask;
            var ids = items.Select(a => a.Id).ToArray();
            JobService.Schedule<Job>(a => a.Delete(ids), TimeSpan.FromSeconds(10));
            UINotifier.Show(L["Delete jobs started!"], UINotifierSeverity.Info);
        };

        DataGridManager.BeforeEditAsync = async (item, isNew) =>
        {
            item.ClusterName = await PveClientService.GetCurrentClusterName();
            VmIdsList = (item.VmIds + "").Split(",").ToList();
            return item;
        };
    }

    //private IEnumerable<string> VmIds2 { get; set; } = new HashSet<string>();

    //private async Task<IEnumerable<string>> SearchFuncVmIds2(string search)
    //{
    //    var ret = (await PveAdminHelper.GetVmsJollyKeys(_pveClient, true, true, true, true, true))
    //                .Where(a => !VmIdsList.Contains(a))
    //                .ToList();

    //    if (!string.IsNullOrWhiteSpace(search))
    //    {
    //        ret = ret.Where(a => a.Contains(search, StringComparison.InvariantCultureIgnoreCase))
    //                 .ToList();

    //        if (!ret.Contains(search)) { ret.Add(search); }
    //    }

    //    return ret;
    //}

    private async Task<IEnumerable<string>> SearchFuncVmIds(string search)
    {
        var ret = (await (await GetPveClient()).GetVmsJollyKeys(true, true, true, true, true, true))
                    .Where(a => !VmIdsList.Contains(a))
                    .ToList();

        if (!string.IsNullOrWhiteSpace(search))
        {
            ret = ret.Where(a => a.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                     .ToList();

            if (!ret.Contains(search)) { ret.Add(search); }
        }

        return ret;
    }

    private async Task Snap(AutoSnapJob item)
    {
        if (await UIMessageBox.ShowQuestionAsync(L["Snap"], L["Execute Snap?"]))
        {
            JobService.Schedule<Job>(a => a.Create(item.Id), TimeSpan.FromSeconds(10));
            UINotifier.Show(L["Snap started!"], UINotifierSeverity.Info);
        }
    }

    private async Task Clean(AutoSnapJob item)
    {
        if (await UIMessageBox.ShowQuestionAsync(L["Clean"], L["Execute Clean?"]))
        {
            JobService.Schedule<Job>(a => a.Clean(item.Id), TimeSpan.FromSeconds(10));
            UINotifier.Show(L["Clean snapshots started!"], UINotifierSeverity.Info);
        }
    }
}