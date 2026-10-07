/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Components.Settings;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public class DialogServiceEx(IStringLocalizer<DialogServiceEx> L,
                             DialogService dialogService,
                             IModuleService moduleService,
                             IPermissionService permissionService) : IDialogServiceEx
{
    public Task<dynamic?> OpenSettingsAsync<T>(string clusterName) where T : ModuleBase
        => OpenSettingsAsync(moduleService.Get<T>()!, clusterName);

    public async Task<dynamic?> OpenSettingsAsync(ModuleBase module, string clusterName)
    {
        if (!await module.HasPermissionEditorSettingsAsync(permissionService, clusterName)) { return null; }

        return await dialogService.OpenSideExAsync<ModuleSettingsDialog>(L["Settings for "].Value + L[module.Link!.Text].Value,
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
}
