/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;

public interface IAuditService
{
    Type Render { get; }

    Task LogAsync(string userName, string action, bool success, string? details = null);

    Task LogAsync(string action, bool success, string? details = null);
}
