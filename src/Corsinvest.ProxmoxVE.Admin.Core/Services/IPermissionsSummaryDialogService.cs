/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface IPermissionsSummaryDialogService
{
    Task ShowForUserAsync(string userId, string displayName);
    Task ShowForAppTokenAsync(Guid appTokenId, string name);
}
