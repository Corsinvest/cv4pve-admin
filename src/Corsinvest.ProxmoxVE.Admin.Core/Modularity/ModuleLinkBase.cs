/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Web;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

namespace Corsinvest.ProxmoxVE.Admin.Core.Modularity;

public class ModuleLinkBase : IEnabled
{
    public ModuleLinkBase(ModuleBase module,
                          string text,
                          string? url = null,
                          bool isExternal = false)
    {
        Module = module;
        Text = text;
        Url = url ?? HttpUtility.UrlEncode(text).ToLower();
        IsExternal = isExternal;

        RealUrl = IsExternal
                    ? Url
                    : string.IsNullOrWhiteSpace(Url)
                            ? Module.BaseUrl
                            : $"{Module.BaseUrl}/{Url}";

        //add permission
        var permissionKey = Module.PermissionLinkBaseKey;
        if (!string.IsNullOrWhiteSpace(Url)) { permissionKey += $".{Url}"; }
        Permission = new(string.Empty, permissionKey, $"{Text}");
    }

    public ModuleBase Module { get; }
    public string Text { get; }
    public string Icon { get; set; } = default!;
    public string IconColor { get; set; } = default!;
    public string Url { get; }
    public string RealUrl { get; }
    public bool Enabled { get; set; } = true;
    public int OrderIndex { get; set; }
    public bool IsExternal { get; }
    public RenderComponentInfo Render { get; set; } = default!;
    public Permission Permission { get; }
    public IList<ModuleLinkBase> Child { get; set; } = [];

    public async Task<bool> HasPermissionAsync(IPermissionService permissionService, string clusterName)
        => await Module.HasPermissionAsync(permissionService, clusterName, Permission);
}
