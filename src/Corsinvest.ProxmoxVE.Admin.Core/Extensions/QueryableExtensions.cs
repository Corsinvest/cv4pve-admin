/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class QueryableExtensions
{
    public static Task<T?> FromIdAsync<T>(this IQueryable<T> source, int id) where T : IId
        => source.Where(a => a.Id == id).FirstOrDefaultAsync();

    public static Task DeleteAsync<T>(this IQueryable<T> source, int id) where T : IId
        => source.Where(a => a.Id == id).ExecuteDeleteAsync();

    public static Task DeleteAsync<T>(this IQueryable<T> source, IEnumerable<int> ids) where T : IId
        => source.Where(a => ids.Contains(a.Id)).ExecuteDeleteAsync();

    public static Task DeleteAsync<T>(this IQueryable<T> source, string clusterName) where T : IClusterName
        => source.Where(a => a.ClusterName == clusterName).ExecuteDeleteAsync();

    public static IQueryable<T> FromClusterName<T>(this IQueryable<T> source, string clusterName) where T : IClusterName
        => source.Where(a => a.ClusterName == clusterName);

    public static IQueryable<T> Where<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, bool condition)
       => condition
           ? source.Where(predicate)
           : source;

    public static IQueryable<T> Where<T>(this IQueryable<T> source, Expression<Func<T, int, bool>> predicate, bool condition)
        => condition
            ? source.Where(predicate)
            : source;

    public static IQueryable<T> Where<T>(this IQueryable<T> source,
                                         bool condition,
                                         Expression<Func<T, bool>> truePredicate,
                                         Expression<Func<T, bool>> falsePredicate)
        => condition
            ? source.Where(truePredicate)
            : source.Where(falsePredicate);

    public static IQueryable<T> Where<T>(this IQueryable<T> source,
                                         bool condition,
                                         Expression<Func<T, int, bool>> truePredicate,
                                         Expression<Func<T, int, bool>> falsePredicate)
        => condition
            ? source.Where(truePredicate)
            : source.Where(falsePredicate);
}
