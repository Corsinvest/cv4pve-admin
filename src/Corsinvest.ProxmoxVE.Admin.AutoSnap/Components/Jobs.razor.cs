/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.BaseUI.DataManager;
using Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Extensions;
using Corsinvest.ProxmoxVE.Admin.Core.Repository;
using Corsinvest.ProxmoxVE.Api.Extension;
using MudExtensions;

namespace Corsinvest.ProxmoxVE.Admin.AutoSnap.Components;

public partial class Jobs
{
    [Parameter] public string Height { get; set; } = default!;

    [Inject] private IDataGridManagerRepository<AutoSnapJob> DataGridManager { get; set; } = default!;
    [Inject] private IJobService JobService { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    private List<string> VmIds { get; set; } = default!;
    private string? AddedValue { get; set; }
    private MudComboBox<string>? RefMudComboBox { get; set; }

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Jobs "];
        DataGridManager.DefaultSort = new() { [nameof(AutoSnapJob.Id)] = false };
        DataGridManager.QueryAsync = async () => await DataGridManager.Repository.ListAsync(new AutoSnapJobSpec(await PveClientService.GetCurrentClusterNameAsync()));

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
            VmIds = (await (await PveClientService.GetClientCurrentClusterAsync())
                         .GetVmIds(true, true, true, true, true, true))
                         .ToList();

            //add customs
            VmIds.AddRange(item.VmIdsList);
            VmIds = VmIds.Distinct().Order().ToList();

            item.ClusterName = await PveClientService.GetCurrentClusterNameAsync();

            return item;
        };
    }

    private async Task AddItem()
    {
        if (string.IsNullOrEmpty(AddedValue)) { return; }
        VmIds.Add(AddedValue);
        AddedValue = null;
        await RefMudComboBox!.ForceRender(true);
    }

    private async Task Snap(AutoSnapJob item)
    {
        if (await UIMessageBox.ShowQuestionAsync(L["Snap"], L["Execute Snap?"]))
        {
            JobService.Schedule<Job>(a => a.Create(item.Id), TimeSpan.FromSeconds(10));
            UINotifier.Show(L["Creating snapshot!"], UINotifierSeverity.Info);
        }
    }

    private async Task Purge(AutoSnapJob item)
    {
        if (await UIMessageBox.ShowQuestionAsync(L["Clean"], L["Execute Clean?"]))
        {
            JobService.Schedule<Job>(a => a.Purge(item.Id), TimeSpan.FromSeconds(10));
            UINotifier.Show(L["Cleaning snapshots!"], UINotifierSeverity.Info);
        }
    }
}