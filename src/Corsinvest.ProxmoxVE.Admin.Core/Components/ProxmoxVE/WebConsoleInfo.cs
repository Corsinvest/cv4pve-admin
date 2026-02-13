/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE;

internal record WebConsoleInfo(string ClusterName, long VmId, string Node, string VmName, VmType VmType, bool XtermJs)
{
    public static WebConsoleInfo? Decode(IFusionCache fusionCache, string key)
    {
        var info = fusionCache.TryGet<WebConsoleInfo>(key);
        if (info.HasValue) { fusionCache.Remove(key); }
        return info.GetValueOrDefault();
    }
}
