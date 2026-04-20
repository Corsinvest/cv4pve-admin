/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

public abstract class BasePermission : IId, IClusterName
{
    private string _path = default!;
    private string[]? _expectedSegmentsCache;

    public int Id { get; set; }
    [Required] public string PermissionKey { get; set; } = default!;

    [Required]
    public string Path
    {
        get => _path;
        set
        {
            _path = value;
            _expectedSegmentsCache = null;
        }
    }

    public bool Propagated { get; set; }
    [Required] public string ClusterName { get; set; } = default!;
    public bool BuiltIn { get; set; }

    public bool MatchesPath(string actualPath)
    {
        if (string.IsNullOrEmpty(Path) || string.IsNullOrEmpty(actualPath)) { return false; }
        if (Path == "*") { return true; }

        var expectedSegments = _expectedSegmentsCache ??= Path.Trim('/').Split('/');
        if (expectedSegments.Length != 2) { return false; }

        var actualSegments = actualPath.Trim('/').Split('/');

        return actualSegments.Length == 2
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
