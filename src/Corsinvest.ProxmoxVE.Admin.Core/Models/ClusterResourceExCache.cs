/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Reflection;

namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public static class ClusterResourceExCache
{
    private static readonly Lazy<string[]> _propertyNames = new(() =>
        [.. typeof(ClusterResourceEx).GetProperties()
                                 .Where(p => p.CanRead && p.CanWrite)
                                 .Select(p => p.Name)]);

    private static readonly Lazy<PropertyInfo[]> _properties = new(() =>
        [.. typeof(ClusterResourceEx).GetProperties().Where(p => p.CanRead && p.CanWrite)]);

    private static readonly Lazy<Dictionary<string, PropertyInfo>> _propertyLookup = new(() =>
        Properties.ToDictionary(p => p.Name, p => p));

    /// <summary>
    /// Gets all property names for ClusterResourceEx (cached)
    /// </summary>
    public static string[] PropertyNames => _propertyNames.Value;

    /// <summary>
    /// Gets all PropertyInfo objects for ClusterResourceEx (cached)
    /// </summary>
    public static PropertyInfo[] Properties => _properties.Value;

    /// <summary>
    /// Gets a dictionary lookup of property name to PropertyInfo (cached)
    /// </summary>
    public static Dictionary<string, PropertyInfo> PropertyLookup => _propertyLookup.Value;

    /// <summary>
    /// Gets a specific PropertyInfo by name (O(1) lookup)
    /// </summary>
    public static PropertyInfo? GetProperty(string name) => PropertyLookup.TryGetValue(name, out var prop) ? prop : null;
}
