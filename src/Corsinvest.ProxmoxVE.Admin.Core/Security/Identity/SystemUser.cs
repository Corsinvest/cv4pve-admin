/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

/// <summary>
/// Special built-in user used by background jobs and other non-HTTP execution
/// contexts (Hangfire cron, hosted services, workflow timers). It has full
/// permissions but cannot log in: created locked-out with no password and an
/// unroutable email address.
/// </summary>
public static class SystemUser
{
    public const string UserName = "system";
    public const string DisplayName = "System";
    public const string Email = "system@cv4pve-admin.invalid";
    public static string Id { get; set; } = string.Empty;
}
