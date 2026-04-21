/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Components.Settings;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public class DialogServiceEx(IStringLocalizer<DialogServiceEx> L,
                             DialogService dialogService,
                             IModuleService moduleService) : IDialogServiceEx
{
    public Task<dynamic?> OpenSettingsAsync<T>(string clusterName) where T : ModuleBase
        => OpenSettingsAsync(moduleService.Get<T>()!, clusterName);

    public Task<dynamic?> OpenSettingsAsync(ModuleBase module, string clusterName)
        => dialogService.OpenSideExAsync<ModuleSettingsDialog>(L["Settings for "].Value + L[module.Link!.Text].Value,
                                                               new()
                                                               {
                                                                   [nameof(ModuleSettingsDialog.Module)] = module,
                                                                   [nameof(ModuleSettingsDialog.ClusterName)] = clusterName
                                                               },
                                                               new()
                                                               {
                                                                   CloseDialogOnOverlayClick = true
                                                               });
}
