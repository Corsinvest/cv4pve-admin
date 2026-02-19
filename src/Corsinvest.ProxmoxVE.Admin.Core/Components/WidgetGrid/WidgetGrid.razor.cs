/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.JSInterop;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.WidgetGrid;

public partial class WidgetGrid(IJSRuntime JS)
{
    private readonly string _gridId = $"widgetgrid-{Guid.NewGuid():N}";
    private DotNetObjectReference<WidgetGrid>? _dotNetRef;
    private bool _needsRefresh;

    [Parameter] public IEnumerable<WidgetGridItem> Items { get; set; } = [];
    [Parameter] public int Rows { get; set; } = 12;
    [Parameter] public int Cols { get; set; } = 12;
    [Parameter] public int Margin { get; set; } = 8;
    [Parameter] public bool EditMode { get; set; }
    [Parameter] public bool ShowGrid { get; set; }
    [Parameter] public string Height { get; set; } = "600px";
    [Parameter] public EventCallback<WidgetGridItem> OnPositionChanged { get; set; }
    [Parameter] public RenderFragment<WidgetGridItem>? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment<WidgetGridItem>? MenuTemplate { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("cv4pve.WidgetGrid.createInstance", _gridId, Rows, Cols, Margin, EditMode, ShowGrid, _dotNetRef);
        }
        else if (_needsRefresh && _dotNetRef != null)
        {
            _needsRefresh = false;
            await JS.InvokeVoidAsync("cv4pve.WidgetGrid.refreshWidgets", _gridId);
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_dotNetRef != null)
        {
            _needsRefresh = true;
            await JS.InvokeVoidAsync("cv4pve.WidgetGrid.updateConfig", _gridId, Rows, Cols, Margin, EditMode, ShowGrid);
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_dotNetRef != null)
            {
                await JS.InvokeVoidAsync("cv4pve.WidgetGrid.destroyInstance", _gridId);
                _dotNetRef.Dispose();
            }
        }
        catch { }
    }

    public async Task RefreshAsync()
    {
        if (_dotNetRef != null)
        {
            await JS.InvokeVoidAsync("cv4pve.WidgetGrid.refreshWidgets", _gridId);
        }
    }

    public static string? NormalizeCss(string? css)
        => string.IsNullOrWhiteSpace(css)
            ? css
            : css.Replace("\r\n", " ")
                 .Replace("\n", " ")
                 .Replace("\r", " ");

    [JSInvokable]
    public async Task UpdateWidgetPosition(int id, int col, int row, int colSpan, int rowSpan)
    {
        var item = Items.FirstOrDefault(w => w.Id == id);
        if (item != null)
        {
            item.Col = col;
            item.Row = row;
            item.ColSpan = colSpan;
            item.RowSpan = rowSpan;

            await OnPositionChanged.InvokeAsync(item);
            StateHasChanged();
        }
    }
}
