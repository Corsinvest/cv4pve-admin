/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

public abstract class BasePermission : IId
{
    public int Id { get; set; }
    [Required] public string PermissionKey { get; set; } = default!;

    [Required] public string Path { get; set; } = default!;
    public bool Propagated { get; set; }
    [Required] public string ClusterName { get; set; } = default!;
    public bool BuiltIn { get; set; }

    public bool MatchesPath(string actualPath)
    {
        if (string.IsNullOrEmpty(Path) || string.IsNullOrEmpty(actualPath)) { return false; }
        if (Path == "*") { return true; }

        var expectedSegments = Path.Trim('/').Split('/');
        var actualSegments = actualPath.Trim('/').Split('/');

        return expectedSegments.Length == 2
                    && actualSegments.Length == 2
                    && string.Equals(expectedSegments[0], actualSegments[0], StringComparison.OrdinalIgnoreCase)
                    && (expectedSegments[1] == "*"
                        || string.Equals(expectedSegments[1], actualSegments[1], StringComparison.OrdinalIgnoreCase));
    }

    public bool HasPermission(string permissionKey, string path, string clusterName)
        => PermissionKey == permissionKey
            && MatchesPath(path)
            && (Propagated || Path == path)
            && (ClusterName == ApplicationHelper.AllClusterName || ClusterName == clusterName);
}
