/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

public interface ISessionStore
{
    // Closes every session of the user: each browser has to sign in again
    Task RemoveForUserAsync(string userId);
}
