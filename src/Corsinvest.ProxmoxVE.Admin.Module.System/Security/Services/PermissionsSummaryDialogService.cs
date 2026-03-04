/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Module.System.Security.Components;
using Microsoft.Extensions.Localization;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Services;

public class PermissionsSummaryDialogService(DialogService dialogService,
                                             IStringLocalizer<PermissionsSummaryDialogService> L)
    : IPermissionsSummaryDialogService
{
    public Task ShowForUserAsync(string userId, string displayName)
        => dialogService.OpenSideExAsync<PermissionsSummaryDialog>(
               L["Effective Permissions — {0}", displayName],
               new() { [nameof(PermissionsSummaryDialog.UserId)] = userId },
               new() { Width = "60%", CloseDialogOnOverlayClick = true });

    public Task ShowForAppTokenAsync(Guid appTokenId, string name)
        => dialogService.OpenSideExAsync<PermissionsSummaryDialog>(
               L["Effective Permissions — {0}", name],
               new() { [nameof(PermissionsSummaryDialog.AppTokeId)] = appTokenId },
               new() { Width = "60%", CloseDialogOnOverlayClick = true });
}
