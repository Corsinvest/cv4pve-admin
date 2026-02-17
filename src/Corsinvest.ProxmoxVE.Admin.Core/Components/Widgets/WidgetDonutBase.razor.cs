/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;

public abstract partial class WidgetDonutBase<TSettings> : IModuleWidget<TSettings>, IDisposable
{
    [Parameter] public TSettings Settings { get; set; } = default!;
    [Parameter] public EventCallback<TSettings> SettingsChanged { get; set; }
    [Parameter] public IEnumerable<string> ClusterNames { get; set; } = [];
    [Parameter] public bool InEditing { get; set; }
    [Parameter] public IList<Item> Items { get; set; } = [];
    [Parameter] public string SerieTitle { get; set; } = default!;
    [Parameter] public RenderFragment? Title { get; set; }
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

    public class Item
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private bool _disposed;

    protected void SetOk(int count) => Set("Ok", count, Colors.Success);
    protected void SetKo(int count) => Set("Ko", count, Colors.Danger);
    protected void Clear() => Items?.Clear();

    protected void Set(string category, int count, string color)
    {
        var item = Items.FirstOrDefault(a => a.Category == category);
        if (item == null)
        {
            item = new()
            {
                Count = 0,
                Category = category,
                Color = color
            };
            Items.Add(item);
        }
        item.Count += count;
    }

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        if (_disposed) { return; }
        if (!await _refreshLock.WaitAsync(0)) { return; }
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
