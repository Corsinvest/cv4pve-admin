/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;

public abstract partial class WidgetInfoBase<TSettings> : IModuleWidget<TSettings>, IDisposable
{
    [Parameter] public TSettings Settings { get; set; } = default!;
    [Parameter] public EventCallback<TSettings> SettingsChanged { get; set; }
    [Parameter] public IEnumerable<string> ClusterNames { get; set; } = [];
    [Parameter] public bool InEditing { get; set; }

    protected IEnumerable<StatCard> Stats { get; set; } = [];
    protected IEnumerable<AlertItem> Alerts { get; set; } = [];
    protected IEnumerable<AgeDistributionItem> AgeDistribution { get; set; } = [];

    protected bool IsLoadingStats { get; set; }
    protected bool IsLoadingAlerts { get; set; }
    protected bool IsLoadingDistribution { get; set; }

    public record StatCard(string Title, string Value, string Subtitle, string Color);
    public record AlertItem(string Icon, string Color, string Text);
    public record AgeDistributionItem(string Label, int Value, int Percent, ProgressBarStyle Style);

    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private bool _disposed;

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        if (_disposed || !await _refreshLock.WaitAsync(0)) { return; }
        try
        {
            await RefreshDataAsyncInt();
        }
        finally
        {
            if (!_disposed) { _refreshLock?.Release(); }
        }
    }

    protected abstract Task RefreshDataAsyncInt();

    public virtual void Dispose()
    {
        _disposed = true;
        _refreshLock?.Dispose();
        GC.SuppressFinalize(this);
    }
}
