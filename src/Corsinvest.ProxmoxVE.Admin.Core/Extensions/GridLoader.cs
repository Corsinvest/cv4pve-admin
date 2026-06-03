/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class GridLoader
{
    public static GridLoader<TSource, TResult> Create<TSource, TResult>(
        RadzenDataGrid<TResult> grid,
        string? defaultOrderBy = null,
        ILogger? logger = null)
        where TSource : class
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(grid);

        defaultOrderBy ??= TryInferDefaultOrderBy(grid);
        return new GridLoader<TSource, TResult>(grid, defaultOrderBy, logger);
    }

    private static string? TryInferDefaultOrderBy<TResult>(RadzenDataGrid<TResult> grid) where TResult : notnull
    {
        if (grid.ColumnsCollection is null) { return null; }

        var parts = grid.ColumnsCollection
                        .Where(c => c.SortOrder.HasValue)
                        .OrderBy(c => c.OrderIndex ?? int.MaxValue)
                        .Select(c =>
                        {
                            var prop = !string.IsNullOrEmpty(c.SortProperty) ? c.SortProperty : c.Property;
                            if (string.IsNullOrEmpty(prop)) { return null; }
                            var dir = c.SortOrder == SortOrder.Descending ? "desc" : "asc";
                            return $"{prop} {dir}";
                        })
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();

        return parts.Count == 0 ? null : string.Join(", ", parts);
    }
}

public sealed partial class GridLoader<TSource, TResult> : IDisposable
    where TSource : class
    where TResult : notnull
{
    [GeneratedRegex(@"^[\w\.\s]+(\s+(asc|desc))?(\s*,\s*[\w\.\s]+(\s+(asc|desc))?)*$",
                    RegexOptions.IgnoreCase)]
    private static partial Regex OrderByValidationRegex();

    private readonly RadzenDataGrid<TResult> _grid;
    private readonly string? _defaultOrderBy;
    private readonly ILogger? _logger;

    private string? _lastFiltersSignature;
    private int _cachedTotalCount;
    private ResultLoadData<TResult>? _lastResult;
    private CancellationTokenSource? _inFlightCts;
    private Func<List<FilterDescriptor>, Task>? _onFiltersChanged;
    private Func<IEnumerable<FilterDescriptor>>? _extraFiltersSource;

    internal GridLoader(RadzenDataGrid<TResult> grid,
                        string? defaultOrderBy,
                        ILogger? logger)
    {
        _grid = grid;
        _defaultOrderBy = defaultOrderBy;
        _logger = logger;
    }

    public void OnFiltersChanged(Func<List<FilterDescriptor>, Task> callback)
        => _onFiltersChanged = callback;

    public void WithExtraFilters(Func<IEnumerable<FilterDescriptor>> source)
        => _extraFiltersSource = source;

    public async Task<ResultLoadData<TResult>> LoadAsync(IQueryable<TSource> query,
                                                         LoadDataArgs args,
                                                         Expression<Func<TSource, TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(selector);

        _inFlightCts?.Cancel();
        _inFlightCts = new CancellationTokenSource();
        var ct = _inFlightCts.Token;

        var extras = _extraFiltersSource?.Invoke() ?? [];
        var allFilters = (args.Filters ?? []).Concat(extras).ToList();
        var signature = ComputeFiltersSignature(allFilters);
        var filtersChanged = signature != _lastFiltersSignature;

        query = query.AsNoTracking()
                     .Where(allFilters, _grid.LogicalFilterOperator, _grid.FilterCaseSensitivity);

        int totalCount;
        if (!filtersChanged)
        {
            totalCount = _cachedTotalCount;
        }
        else
        {
            try
            {
                totalCount = await query.CountAsync(ct);
            }
            catch (OperationCanceledException)
            {
                return _lastResult ?? Empty();
            }

            _lastFiltersSignature = signature;
            _cachedTotalCount = totalCount;
        }

        var orderBy = Validate(args.OrderBy) ?? Validate(_defaultOrderBy);
        if (!string.IsNullOrEmpty(orderBy))
        {
            query = query.OrderBy(orderBy);
        }

        ResultLoadData<TResult> result;
        try
        {
            var items = await query.Select(selector)
                                   .Skip(args.Skip ?? 0)
                                   .Take(args.Top ?? 50)
                                   .ToListAsync(ct);
            result = new(items, totalCount, args.Filter);
            _lastResult = result;
        }
        catch (OperationCanceledException)
        {
            return _lastResult ?? Empty();
        }

        if (filtersChanged && _onFiltersChanged is not null)
        {
            try { await _onFiltersChanged(allFilters); }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Companion OnFiltersChanged callback threw");
            }
        }

        return result;
    }

    public void InvalidateFiltersCache() => _lastFiltersSignature = null;

    public Task RefreshAsync()
    {
        InvalidateFiltersCache();
        return _grid.Reload();
    }

    public void Dispose()
    {
        _inFlightCts?.Cancel();
        _inFlightCts?.Dispose();
        _inFlightCts = null;
    }

    private string? Validate(string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy)) { return null; }
        if (OrderByValidationRegex().IsMatch(orderBy)) { return orderBy; }
        _logger?.LogWarning("Potentially unsafe OrderBy clause rejected: {OrderBy}", orderBy);
        return null;
    }

    private static string ComputeFiltersSignature(List<FilterDescriptor> filters)
        => string.Join("|", filters.Select(f =>
            $"{f.Property}={f.FilterValue}:{f.FilterOperator}:{f.LogicalFilterOperator}"));

    private static ResultLoadData<TResult> Empty() => new([], 0, null);
}
