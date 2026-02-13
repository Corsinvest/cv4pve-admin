/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class QueryableExtensions
{
    public static async Task<T?> FromIdAsync<T>(this IQueryable<T> source, int id) where T : IId
        => await source.Where(a => a.Id == id).FirstOrDefaultAsync();

    public static async Task DeleteAsync<T>(this IQueryable<T> source, int id) where T : IId
        => await source.Where(a => a.Id == id).ExecuteDeleteAsync();

    public static async Task DeleteAsync<T>(this IQueryable<T> source, IEnumerable<int> ids) where T : IId
        => await source.Where(a => ids.Contains(a.Id)).ExecuteDeleteAsync();

    public static async Task DeleteAsync<T>(this IQueryable<T> source, string clusterName) where T : IClusterName
        => await source.Where(a => a.ClusterName == clusterName).ExecuteDeleteAsync();

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

    public static async Task<ResultLoadData<TResult>> LoadDataAsync<TSource, TResult>(this IQueryable<TSource> query,
                                                                                      LoadDataArgs args,
                                                                                      RadzenDataGrid<TResult> grid,
                                                                                      Expression<Func<TSource, TResult>> selector,
                                                                                      string? lastFilter,
                                                                                      ILogger? logger = null)
         where TResult : notnull
         where TSource : class
    {
        var skip = args.Skip ?? 0;
        var take = args.Top ?? 50;

        var newFilter = lastFilter;

        if (!string.IsNullOrEmpty(args.Filter) && lastFilter != args.Filter)
        {
            skip = 0;
            newFilter = args.Filter;
        }

        query = query.AsNoTracking()
                     .Where(args.Filters!, grid.LogicalFilterOperator, grid.FilterCaseSensitivity);

        var totalCount = await query.CountAsync();

        // Validate OrderBy to prevent SQL injection via dynamic LINQ
        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            // Basic validation: only allow alphanumeric, dots, spaces, and "asc"/"desc"
            if (System.Text.RegularExpressions.Regex.IsMatch(args.OrderBy, @"^[\w\.\s,]+(\s+(asc|desc))?$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                query = query.OrderBy(args.OrderBy);
            }
            else
            {
                logger?.LogWarning("Potentially unsafe OrderBy clause rejected: {OrderBy}", args.OrderBy);
            }
        }

        var items = await query.Select(selector)
                               .Skip(skip)
                               .Take(take)
                               .ToListAsync();

        return new(items, totalCount, newFilter);
    }
}
