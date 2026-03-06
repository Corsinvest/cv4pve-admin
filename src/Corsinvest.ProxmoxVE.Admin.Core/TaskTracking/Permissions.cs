/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

namespace Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;

public static class Permissions
{
    private static string BaseName { get; } = "Tasks";
    public static Permission Read { get; } = new(BaseName, nameof(Read), "View tasks");
    public static Permission Stop { get; } = new(BaseName, nameof(Stop), "Cancel running tasks");
}
