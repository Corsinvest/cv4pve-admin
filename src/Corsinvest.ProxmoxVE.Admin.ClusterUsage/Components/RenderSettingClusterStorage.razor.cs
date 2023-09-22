/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.BaseUI.DataManager;
using Corsinvest.ProxmoxVE.Api.Extension;

namespace Corsinvest.ProxmoxVE.Admin.ClusterUsage.Components;

public partial class RenderSettingClusterStorage
{
    [EditorRequired][Parameter] public ModuleClusterOptions ModuleClusterOptions { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;
    [Inject] private IDataGridManager<StorageOptions> DataGridManager { get; set; } = default!;

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Storages"];
        DataGridManager.DefaultSort = new() { [nameof(StorageOptions.Storage)] = false };
        DataGridManager.QueryAsync = async () =>
        {
            //load existing storages
            var client = await PveClientService.GetClientAsync(ModuleClusterOptions.ClusterName);
            foreach (var item in await client.GetStorages())
            {
                if (!ModuleClusterOptions.Storages.Any(a => a.Storage == item.Storage))
                {
                    ModuleClusterOptions.Storages.Add(new() { Storage = item.Storage });
                }
            }

            return ModuleClusterOptions.Storages;
        };
    }
}