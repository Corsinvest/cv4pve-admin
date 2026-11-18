/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

public record DailyChartPoint<TCategory>(DateTime Day, TCategory Category, int Count);

/// <summary>Days chosen in the chart and, after a click on a bar, the category of that bar.</summary>
public record DailyChartFilter<TCategory>(DateTime From, DateTime To, bool HasCategory, TCategory Category);

public partial class DailyChart<TCategory> where TCategory : notnull
{
    private record TotalPoint(DateTime Day, int Count);

    private const double Epsilon = 1e-9;

    [Parameter, EditorRequired] public string Title { get; set; } = default!;
    [Parameter] public List<DailyChartPoint<TCategory>> Data { get; set; } = [];

    /// <summary>Order of the series; categories missing here follow in the order they appear in the data.</summary>
    [Parameter] public IEnumerable<TCategory> Categories { get; set; } = [];

    /// <summary>Fill of a series; null leaves the colour of the chart scheme.</summary>
    [Parameter] public Func<TCategory, string>? CategoryColor { get; set; }

    [Parameter] public Func<TCategory, string>? CategoryText { get; set; }
    [Parameter] public string? ErrorMessage { get; set; }
    [Parameter] public string? EmptyText { get; set; }
    [Parameter] public bool Loading { get; set; }

    /// <summary>Days selected at first in the range bar; 0 selects every day.</summary>
    [Parameter] public int InitialDays { get; set; }

    /// <summary>
    /// With a handler the range bar is shown and a click on a bar selects its day and category.
    /// Null means no filter: the range bar was never moved and no bar is selected.
    /// </summary>
    [Parameter] public EventCallback<DailyChartFilter<TCategory>?> OnFilterChanged { get; set; }

    private bool Interactive => OnFilterChanged.HasDelegate;
    private List<TotalPoint> TotalsPerDay { get; set; } = [];
    private List<DailyChartPoint<TCategory>> FilteredData { get; set; } = [];
    private IEnumerable<TCategory> OrderedCategories => Categories.Concat(Data.Select(a => a.Category)).Distinct();
    private DailyChartPoint<TCategory>? Selected { get; set; }

    private double _start;
    private double _end = 1.0;
    private DateTime _min;
    private DateTime _max;
    private DateTime _from;
    private DateTime _to;
    private bool _rangeMoved;
    private (double Start, double End) _savedWindow = (0, 1);
    private List<DailyChartPoint<TCategory>>? _lastData;

    protected override void OnParametersSet()
    {
        if (ReferenceEquals(_lastData, Data)) { return; }
        _lastData = Data;

        if (Data.Count == 0)
        {
            TotalsPerDay = [];
            FilteredData = [];
            return;
        }

        var min = Data.Min(p => p.Day);
        var max = Data.Max(p => p.Day);

        // New data over the same days (the page reloads it with its filters) keeps the selected range
        if (min != _min || max != _max)
        {
            _min = min;
            _max = max;
            var total = (_max - _min).TotalDays;
            _start = InitialDays > 0 && total > InitialDays ? 1.0 - (InitialDays / total) : 0;
            _end = 1.0;
        }

        TotalsPerDay = [.. Data.GroupBy(p => p.Day)
                               .Select(g => new TotalPoint(g.Key, g.Sum(p => p.Count)))
                               .OrderBy(p => p.Day)];
        RecomputeWindow();
    }

    private string GetText(TCategory category) => CategoryText?.Invoke(category) ?? category.ToString() ?? string.Empty;

    private async Task OnStartChangedAsync(double value)
    {
        if (Math.Abs(value - _start) < Epsilon) { return; }
        _start = value;
        await RangeChangedAsync();
    }

    private async Task OnEndChangedAsync(double value)
    {
        if (Math.Abs(value - _end) < Epsilon) { return; }
        _end = value;
        await RangeChangedAsync();
    }

    private async Task RangeChangedAsync()
    {
        // Moving the range bar while a bar is selected drops the selection: the new range applies
        Selected = null;
        _rangeMoved = true;
        RecomputeWindow();
        await NotifyAsync();
    }

    private async Task OnSeriesClickAsync(SeriesClickEventArgs args)
    {
        if (!Interactive || args.Data is not DailyChartPoint<TCategory> point) { return; }

        if (Selected == null) { _savedWindow = (_start, _end); }
        Selected = point;

        // The range bar shows the selected day; the chart keeps the days it showed, so the context stays
        var total = (_max - _min).TotalDays;
        if (total > 0)
        {
            _start = Math.Clamp((point.Day - _min).TotalDays / total, 0, 1);
            _end = Math.Clamp((point.Day.AddDays(1) - _min).TotalDays / total, _start, 1);
        }

        await NotifyAsync();
    }

    private async Task ClearSelectionAsync()
    {
        Selected = null;
        (_start, _end) = _savedWindow;
        await NotifyAsync();
    }

    private Task NotifyAsync()
    {
        DailyChartFilter<TCategory>? filter = Selected != null
                                                ? new(Selected.Day, Selected.Day.AddDays(1).AddTicks(-1), true, Selected.Category)
                                                : _rangeMoved
                                                    ? new(_from, _to, false, default!)
                                                    : null;
        return OnFilterChanged.InvokeAsync(filter);
    }

    private void RecomputeWindow()
    {
        if (!Interactive || _max <= _min)
        {
            FilteredData = [.. Data];
            return;
        }

        var total = (_max - _min).TotalDays;
        var (start, end) = Selected == null ? (_start, _end) : _savedWindow;
        _from = _min.AddDays(total * start).Date;
        _to = _min.AddDays(total * end).Date.AddDays(1).AddTicks(-1);
        FilteredData = [.. Data.Where(p => p.Day >= _from && p.Day <= _to)];
    }
}
