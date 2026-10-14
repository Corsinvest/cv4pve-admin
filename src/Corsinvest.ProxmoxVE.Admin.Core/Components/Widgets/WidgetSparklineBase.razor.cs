/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;

public abstract partial class WidgetSparklineBase<TItem, TSettings> : IModuleWidget<TSettings>, IDisposable
{
    [Parameter] public TSettings Settings { get; set; } = default!;
    [Parameter] public EventCallback<TSettings> SettingsChanged { get; set; }
    [Parameter] public IEnumerable<string> ClusterNames { get; set; } = [];
    [Parameter] public bool InEditing { get; set; }
    [Parameter] public IEnumerable<TItem> Items { get; set; } = [];
    [Parameter] public string SerieTitle { get; set; } = default!;
    [Parameter] public string CategoryProperty { get; set; } = default!;
    [Parameter] public string ValueProperty { get; set; } = default!;
    [Parameter] public Func<object, string>? ValueFormatter { get; set; }
    [Parameter] public RenderFragment? Footer { get; set; }

    protected DateTime? LastExecution { get; set; }

    protected void MakeFooterText(string text)
        => Footer = builder =>
        {
            builder.OpenComponent<RadzenText>(0);
            builder.AddAttribute(1, "TextStyle", TextStyle.Caption);
            builder.AddAttribute(2, "class", "rz-text-align-center rz-m-0");
            builder.AddAttribute(3, "Text", text);
            builder.CloseComponent();
        };

    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private bool _disposed;

    protected override Task OnInitializedAsync() => RefreshDataAsync();

    // A refresh asked while one is running is not dropped: it runs once that one ends,
    // so the last change of the data is always shown.
    private int _refreshPending;

    public async Task RefreshDataAsync()
    {
        if (_disposed) { return; }
        if (!await _refreshLock.WaitAsync(0))
        {
            Interlocked.Exchange(ref _refreshPending, 1);
            return;
        }

        try
        {
            Interlocked.Exchange(ref _refreshPending, 0);
            await RefreshDataAsyncInt();
        }
        finally
        {
            if (!_disposed) { _refreshLock?.Release(); }
        }

        if (!_disposed && Interlocked.Exchange(ref _refreshPending, 0) == 1) { await RefreshDataAsync(); }
    }

    protected abstract Task RefreshDataAsyncInt();

    public virtual void Dispose()
    {
        _disposed = true;
        _refreshLock?.Dispose();
        GC.SuppressFinalize(this);
    }
}
