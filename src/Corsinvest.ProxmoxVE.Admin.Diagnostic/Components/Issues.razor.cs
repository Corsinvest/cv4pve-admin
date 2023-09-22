/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Services;
using Corsinvest.ProxmoxVE.Admin.Diagnostic.Repository;

namespace Corsinvest.ProxmoxVE.Admin.Diagnostic.Components;

public partial class Issues
{
    [Parameter] public string Height { get; set; } = default!;

    [Inject] private IDataGridManagerRepository<IgnoredIssue> DataGridManager { get; set; } = default!;
    [Inject] private IJobService JobService { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Ignored Issues"];
        DataGridManager.DefaultSort = new()
        {
            [nameof(IgnoredIssue.Context)] = false,
            [nameof(IgnoredIssue.SubContext)] = false
        };

        DataGridManager.QueryAsync = async ()
            => await DataGridManager.Repository.ListAsync(new IgnoredIssueSpec(await PveClientService.GetCurrentClusterNameAsync()));

        DataGridManager.BeforeEditAsync = async (item, isNew) =>
        {
            item.ClusterName = await PveClientService.GetCurrentClusterNameAsync();
            return item;
        };

        DataGridManager.DeleteAfterAsync = async (items) => await Rescan();
        DataGridManager.SaveAfterAsync = async (item, isNew) => await Rescan();
    }

    private async Task Rescan()
    {
        Job.ScheduleRescan(JobService, await PveClientService.GetCurrentClusterNameAsync());
        UINotifier.Show(L["Rescan jobs started!"], UINotifierSeverity.Info);
    }
}