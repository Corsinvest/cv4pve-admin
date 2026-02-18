/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.JSInterop;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

public partial class RangeSelector(IJSRuntime jSRuntime) : IAsyncDisposable
{
    public record TickMark(double Percent, string Label);

    private readonly string _id = $"cv4pve-rs-{Guid.NewGuid():N}";
    private DotNetObjectReference<RangeSelector>? _dotNetRef;

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public double StartPercent { get; set; } = 0;
    [Parameter] public double EndPercent { get; set; } = 100;
    [Parameter] public IEnumerable<TickMark>? Ticks { get; set; }
    /// <summary>
    /// All data point labels (one per data point, ordered).
    /// JS uses this array to update the handle labels during drag without any SignalR traffic.
    /// </summary>
    [Parameter] public IEnumerable<string>? Labels { get; set; }
    [Parameter] public EventCallback<(double Start, double End)> OnRangeChanged { get; set; }
    /// <summary>
    /// When true, fires OnRangeChanged during drag (more SignalR traffic).
    /// Default false: fires only on mouse/touch release.
    /// </summary>
    [Parameter] public bool RealtimeUpdate { get; set; } = false;
    /// <summary>
    /// Debounce delay in ms for RealtimeUpdate. 0 = no debounce. Default 150.
    /// Ignored when RealtimeUpdate is false.
    /// </summary>
    [Parameter] public int RealtimeUpdateDebounceMs { get; set; } = 150;

    private static string TickStyle(double percent)
        => FormattableString.Invariant($"left:{percent:F2}%");

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            await jSRuntime.InvokeVoidAsync("cv4pve.RangeSelector.createInstance",
                _id, _dotNetRef, StartPercent, EndPercent, Labels ?? [], RealtimeUpdate, RealtimeUpdateDebounceMs);
        }
    }

    [JSInvokable]
    public async Task OnRangeChangedJs(double startPct, double endPct)
    {
        StartPercent = startPct;
        EndPercent = endPct;
        await OnRangeChanged.InvokeAsync((startPct, endPct));
    }

    public async ValueTask DisposeAsync()
    {
        try { await jSRuntime.InvokeVoidAsync("cv4pve.RangeSelector.destroyInstance", _id); }
        catch (JSDisconnectedException) { }
        _dotNetRef?.Dispose();
    }
}
