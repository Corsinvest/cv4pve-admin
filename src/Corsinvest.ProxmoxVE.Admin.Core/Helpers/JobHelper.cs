/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class JobHelper
{
    public static string GetJobId<T>(string clusterName, params object?[] args)
        => $"{typeof(T).FullName}-{clusterName}" +
                args == null
                 ? ""
                 : "-" + args.JoinAsString("-");
}