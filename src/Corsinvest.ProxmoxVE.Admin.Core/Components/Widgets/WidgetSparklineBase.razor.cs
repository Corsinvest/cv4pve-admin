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

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        if (!await _refreshLock.WaitAsync(0)) { return; }
        try
        {
            await RefreshDataAsyncInt();
        }
        finally
        {
            _refreshLock?.Release();
        }
    }

    protected abstract Task RefreshDataAsyncInt();

    public virtual void Dispose()
    {
        _refreshLock?.Dispose();
        GC.SuppressFinalize(this);
    }
}
