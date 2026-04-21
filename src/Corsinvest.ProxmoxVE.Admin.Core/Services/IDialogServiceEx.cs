/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface IDialogServiceEx
{
    Task<dynamic?> OpenSettingsAsync(ModuleBase module, string clusterName);
    Task<dynamic?> OpenSettingsAsync<T>(string clusterName) where T : ModuleBase;
}
