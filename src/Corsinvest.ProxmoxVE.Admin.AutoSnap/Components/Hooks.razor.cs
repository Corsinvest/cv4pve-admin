/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.BaseUI.DataManager;
using Corsinvest.AppHero.Core.Domain.Repository;
using Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Repository;

namespace Corsinvest.ProxmoxVE.Admin.AutoSnap.Components;

public partial class Hooks
{
    [Parameter] public string Height { get; set; } = default!;
    [Parameter] public int JobId { get; set; }

    [Inject] private IDataGridManagerRepository<AutoSnapJobHook> DataGridManager { get; set; } = default!;
    [Inject] private IRepository<AutoSnapJob> AutoSnapJobRepo { get; set; } = default!;

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Hooks "];
        DataGridManager.DefaultSort = new() { [nameof(AutoSnapJobHook.Id)] = false };
        DataGridManager.QueryAsync = async () => await DataGridManager.Repository.ListAsync(new AutoSnapJobHookSpec(JobId));

        DataGridManager.BeforeEditAsync = async (item, isNew) =>
        {
            item.Job = (await AutoSnapJobRepo.GetByIdAsync(JobId))!;
            return item;
        };
    }
}