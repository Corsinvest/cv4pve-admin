/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.BackgroundJobs;

public static class Permissions
{
    private static string BaseName { get; } = "BackgroundJobs";
    internal static Permission Dashboard { get; } = new(BaseName, "Dashboard", "Show dashboard Hangfire");
}
