/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Search;

public record SearchContext(
    string RawQuery,
    string SearchText,
    Dictionary<SearchFilter, string> Filters,
    string ClusterName)
{
    /// <summary>
    /// Check if this is a command search (starts with ">")
    /// </summary>
    public bool IsCommandSearch => RawQuery.TrimStart().StartsWith('>');

    /// <summary>
    /// Try to get filter value by filter definition
    /// </summary>
    public bool TryGetFilter(SearchFilter filter, out string? value) => Filters.TryGetValue(filter, out value);

    /// <summary>
    /// Get filter by Id
    /// </summary>
    public SearchFilter? GetFilterById(string id)
        => Filters.Keys.FirstOrDefault(f => f.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}
