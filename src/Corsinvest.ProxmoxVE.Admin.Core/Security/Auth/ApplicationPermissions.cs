/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;

public static class ApplicationPermissions
{
    public static string BaseName { get; } = "Application";

    public static Permission Upgrade { get; } = new(BaseName, "Upgrade", "Upgrade Application");

    public static Role Role { get; } = new($"{BaseName}",
                                           "App",
                                           false,
                                           true,
                                           [Upgrade]);
}
