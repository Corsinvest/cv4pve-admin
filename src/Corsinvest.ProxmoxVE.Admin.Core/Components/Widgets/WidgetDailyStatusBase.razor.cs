/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;

public abstract partial class WidgetDailyStatusBase<TNotification>(EventNotificationService eventNotificationService)
    : IModuleWidget<object>, IDisposable
    where TNotification : IEventNotification
{
    protected const int DaysRange = 14;

    public record StatusEntry(DateTime Start, bool Status);
    public record DayPoint(string DayLabel, int Success, int Failed);

    [Parameter] public object Settings { get; set; } = default!;
    [Parameter] public EventCallback<object> SettingsChanged { get; set; }
    [Parameter] public IEnumerable<string> ClusterNames { get; set; } = [];
    [Parameter] public bool InEditing { get; set; }

    protected List<DayPoint> Points { get; set; } = [];
    protected bool Loading { get; set; } = true;
    private bool _disposed;
    private int _renderKey;
    private int _lastRenderedKey;

    protected abstract Task<List<StatusEntry>> LoadEntriesAsync(DateTime cutoff);

    protected override bool ShouldRender()
    {
        if (_renderKey == _lastRenderedKey) { return false; }
        _lastRenderedKey = _renderKey;
        return true;
    }

    protected override async Task OnInitializedAsync()
    {
        eventNotificationService.Subscribe<TNotification>(HandleDataChangedNotificationAsync);
        await RefreshDataAsync();
    }

    private async Task HandleDataChangedNotificationAsync(TNotification _)
        => await InvokeAsync(async () =>
        {
            await RefreshDataAsync();
            StateHasChanged();
        });

    public async Task RefreshDataAsync()
    {
        if (_disposed) { return; }
        Loading = true;
        try
        {
            var cutoff = DateTime.UtcNow.Date.AddDays(-DaysRange);
            var entries = await LoadEntriesAsync(cutoff);

            Points = [.. entries
                .GroupBy(e => e.Start.Date)
                .OrderBy(g => g.Key)
                .Select(g => new DayPoint(
                    DayLabel: g.Key.ToString("MM-dd"),
                    Success: g.Count(e => e.Status),
                    Failed: g.Count(e => !e.Status)))];
            _renderKey++;
        }
        finally
        {
            Loading = false;
            _renderKey++;
        }
    }

    public void Dispose()
    {
        _disposed = true;
        eventNotificationService.Unsubscribe<TNotification>(HandleDataChangedNotificationAsync);
        GC.SuppressFinalize(this);
    }
}
