/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public class PvePermissions(IReadOnlyDictionary<string, IReadOnlyList<string>> data)
{
    public bool Has(string permission, string path = "/")
        => data.TryGetValue(path, out var list) && list.Contains(permission);

    public bool HasAny(IEnumerable<string> permissions, string path = "/")
        => permissions.Any(p => Has(p, path));

    public bool HasAll(IEnumerable<string> permissions, string path = "/")
        => permissions.All(p => Has(p, path));
}
