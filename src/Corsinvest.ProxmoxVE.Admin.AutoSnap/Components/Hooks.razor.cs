/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.BaseUI.DataManager;
using Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.AutoSnap.Api;
using Microsoft.JSInterop;

namespace Corsinvest.ProxmoxVE.Admin.AutoSnap.Components;

public partial class Hooks
{
    [Parameter] public string Height { get; set; } = default!;
    [Parameter] public AutoSnapJob Job { get; set; } = default!;

    [Inject] private IDataGridManager<AutoSnapJobHook> DataGridManager { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private string HookEnv { get; set; } = default!;

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Hooks "];
        DataGridManager.DefaultSort = new() { [nameof(AutoSnapJobHook.Id)] = false };
        DataGridManager.QueryAsync = async () => await Task.FromResult(Job.Hooks);

        DataGridManager.SaveAsync = async (item, isNew) =>
        {
            if (isNew) { Job.Hooks.Add(item); }
            return await Task.FromResult(true);
        };

        DataGridManager.DeleteAsync = async (items) =>
        {
            foreach (var item in items) { Job.Hooks.Remove(item); }
            return await Task.FromResult(true);
        };

        DataGridManager.BeforeEditAsync = async (item, isNew) =>
        {
            item.Job = Job;
            return await Task.FromResult(item);
        };
    }

    private async Task OnClickHookEnv() => await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", $"%{HookEnv}%");

    private static ICollection<string> GetEnvironments()
        => new PhaseEventArgs(HookPhase.SnapJobStart, new ClusterResource(), null, 0, null, false, 0, false)
                .Environments.Keys.ToList();
}