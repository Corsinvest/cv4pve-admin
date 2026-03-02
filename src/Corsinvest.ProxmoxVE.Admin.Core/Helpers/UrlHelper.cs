/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class UrlHelper
{
    public const string ModuleComponentUrl = "/module/";

    public static string UrlNewPveConfig { get; set; } = default!;
    public static string UrlChangePassword { get; set; } = default!;

    public static class Resources
    {
        public static string VmUrl(long vmId) => $"{ModuleComponentUrl}resources/vms?vmid={vmId}";
        public static string NodeUrl(string node) => $"{ModuleComponentUrl}resources/nodes?node={node}";
    }
}
