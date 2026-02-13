/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    /// Traverses a tree structure depth-first. Protects against circular references.
    /// </summary>
    public static IEnumerable<T> Traverse<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> childSelector)
    {
        var visited = new HashSet<T>();
        var stack = new Stack<T>(items);

        while (stack.Count != 0)
        {
            var next = stack.Pop();

            if (!visited.Add(next))
            {
                continue; // Skip if already visited (prevents infinite loops)
            }

            yield return next;

            foreach (var child in childSelector(next))
            {
                if (!visited.Contains(child))
                {
                    stack.Push(child);
                }
            }
        }
    }

    public static IEnumerable<T> IsEnabled<T>(this IEnumerable<T> items) where T : IEnabled
        => items.Where(a => a.Enabled);

    public static T? FromClusterName<T>(this IEnumerable<T> query, string name) where T : IClusterName
        => query.FirstOrDefault(a => a.ClusterName == name);

    public static T? FromName<T>(this IEnumerable<T> items, string name) where T : IName
        => items.FirstOrDefault(a => a.Name == name);

    public static string JoinAsString<T>(this IEnumerable<T> source, string separator) => string.Join(separator, source);

    public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, bool> predicate, bool condition)
        => condition
            ? source.Where(predicate)
            : source;

    public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, int, bool> predicate, bool condition)
        => condition
            ? source.Where(predicate)
            : source;

    public static IEnumerable<T> Where<T>(this IEnumerable<T> source,
                                          Func<T, bool> truePredicate,
                                          Func<T, bool> falsePredicate,
                                          bool condition)
        => condition
            ? source.Where(truePredicate)
            : source.Where(falsePredicate);

    public static IEnumerable<T> Where<T>(this IEnumerable<T> source,
                                            bool condition,
                                            Func<T, int, bool> truePredicate,
                                            Func<T, int, bool> falsePredicate)
        => condition
            ? source.Where(truePredicate)
            : source.Where(falsePredicate);
}
