/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Wangkanai.Extensions;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Components;

public partial class PermissionsSummaryDialog(IModuleService moduleService,
                                              IDbContextFactory<ModuleDbContext> dbContextFactory)
{
    private enum PermissionSource { Direct, Role, Both }

    private record ResolvedPermission(string PermissionKey,
                                     string Description,
                                     string ClusterName,
                                     string Path,
                                     PermissionSource Source,
                                     string? RoleName)
    {
        public string SourceLabel => Source == PermissionSource.Direct
                                        ? "Direct"
                                        : $"Role: {RoleName}";
    }

    [Parameter] public string? UserId { get; set; }
    [Parameter] public Guid? AppTokeId { get; set; }

    private IEnumerable<ResolvedPermission> Items { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();

        var allPermissions = moduleService.Modules
                                          .SelectMany(a => a.AllRoles)
                                          .SelectMany(a => a.Permissions)
                                          .DistinctBy(a => a.Key)
                                          .ToDictionary(p => p.Key, p => p.Description);

        if (!string.IsNullOrEmpty(UserId))
        {
            var direct = await db.UserPermissions
                                 .Where(a => a.UserId == UserId)
                                 .ToListAsync();

            var fromRoles = await db.UserRoles
                                    .Where(a => a.UserId == UserId)
                                    .SelectMany(a => a.Role.RolePermissions.Select(p => new { RoleName = a.Role.Name, Permission = p }))
                                    .ToListAsync();

            Items = BuildItems(direct, fromRoles.Select(r => (r.RoleName, (BasePermission)r.Permission)), allPermissions);
        }
        else if (AppTokeId != null)
        {
            var direct = await db.AppTokenPermissions
                                 .Where(a => a.AppTokenId == AppTokeId)
                                 .ToListAsync();

            var fromRoles = await db.AppTokenRoles
                                    .Where(a => a.AppTokenId == AppTokeId)
                                    .SelectMany(a => a.Role.RolePermissions.Select(p => new { RoleName = a.Role.Name, Permission = p }))
                                    .ToListAsync();

            Items = BuildItems(direct, fromRoles.Select(r => (r.RoleName, (BasePermission)r.Permission)), allPermissions);
        }
    }

    private static IEnumerable<ResolvedPermission> BuildItems(IEnumerable<BasePermission> direct,
                                                              IEnumerable<(string? RoleName, BasePermission Permission)> fromRoles,
                                                              Dictionary<string, string> allPermissions)
        => direct.Select(p => new ResolvedPermission(p.PermissionKey,
                                                     allPermissions.GetValueOrDefault(p.PermissionKey, p.PermissionKey),
                                                     p.ClusterName,
                                                     p.Path,
                                                     PermissionSource.Direct,
                                                     null))
                 .Concat(fromRoles.Select(r => new ResolvedPermission(r.Permission.PermissionKey,
                                                                      allPermissions.GetValueOrDefault(r.Permission.PermissionKey, r.Permission.PermissionKey),
                                                                      r.Permission.ClusterName,
                                                                      r.Permission.Path,
                                                                      PermissionSource.Role,
                                                                      r.RoleName)));

    private IEnumerable<TreeNode> RootNodes { get; set; } = [];
    private Dictionary<string, TreeNode> NodeMap { get; set; } = [];

    protected override Task OnParametersSetAsync()
    {
        NodeMap = BuildNodeMap();
        RootNodes = [.. NodeMap.Values
                               .Where(n => !n.PathKey.Contains('.'))
                               .OrderBy(n => n.Label)];
        return Task.CompletedTask;
    }

    private Dictionary<string, TreeNode> BuildNodeMap()
    {
        var allPermissions = moduleService.Modules
                                          .SelectMany(a => a.AllRoles)
                                          .SelectMany(a => a.Permissions)
                                          .DistinctBy(a => a.Key)
                                          .ToDictionary(p => p.Key, p => p.Description);

        var map = new Dictionary<string, TreeNode>();

        foreach (var item in Items)
        {
            var segments = item.PermissionKey.Split('.');
            for (var i = 0; i < segments.Length; i++)
            {
                var pathKey = string.Join('.', segments[..(i + 1)]);
                var isLeaf = i == segments.Length - 1;

                if (!map.TryGetValue(pathKey, out var node))
                {
                    node = new TreeNode
                    {
                        PathKey = pathKey,
                        Label = isLeaf
                            ? (allPermissions.TryGetValue(item.PermissionKey, out var desc) ? desc : item.PermissionKey)
                            : segments[i],
                        PermissionKey = isLeaf ? item.PermissionKey : null
                    };
                    map[pathKey] = node;
                }

                if (isLeaf)
                {
                    // Merge source: if both direct and role → Both
                    if (node.Source == null)
                    {
                        node.Source = item.Source;
                        node.RoleName = item.RoleName;
                        node.ClusterName = item.ClusterName;
                        node.Path = item.Path;
                    }
                    else if (node.Source != item.Source)
                    {
                        node.Source = PermissionSource.Both;
                        node.RoleName = null;
                    }
                }
            }
        }

        return map;
    }

    private void TreeRowRender(RowRenderEventArgs<TreeNode> args)
        => args.Expandable = NodeMap.Keys.Any(k => k.StartsWith(args.Data!.PathKey + '.') &&
                                                    k.Count(c => c == '.') == args.Data.PathKey.Count(c => c == '.') + 1);

    private void TreeLoadChildData(DataGridLoadChildDataEventArgs<TreeNode> args)
    {
        var parentKey = args.Item!.PathKey;
        var depth = parentKey.Count(c => c == '.') + 1;

        args.Data = [.. NodeMap.Values
                               .Where(n => n.PathKey.StartsWith(parentKey + '.') && n.PathKey.Count(c => c == '.') == depth)
                               .OrderBy(n => n.Label)];
    }

    private class TreeNode
    {
        public string PathKey { get; set; } = default!;
        public string Label { get; set; } = default!;
        public string? PermissionKey { get; set; }
        public bool IsLeaf => PermissionKey != null;
        public PermissionSource? Source { get; set; }
        public string? RoleName { get; set; }
        public string? ClusterName { get; set; }
        public string? Path { get; set; }
        public string? SourceLabel => IsLeaf
            ? (Source == PermissionSource.Direct ? "Direct" : $"Role: {RoleName}")
            : null;
    }
}
