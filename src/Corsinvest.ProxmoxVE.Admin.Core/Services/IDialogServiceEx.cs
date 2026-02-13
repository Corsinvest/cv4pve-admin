/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface IDialogServiceEx
{
    Task OpenSettingsAsync(ModuleBase module, string clusterName);
    Task OpenSettingsAsync<T>(string clusterName) where T : ModuleBase;
}
