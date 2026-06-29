/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Net.Mime;
using System.Text.Json;
using Corsinvest.ProxmoxVE.Admin.Core.Configuration;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace Corsinvest.ProxmoxVE.Admin.Module.Dashboard.Components;

public partial class Dashboard(IDbContextFactory<ModuleDbContext> dbContextFactory,
                               IModuleService moduleService,
                               ICurrentUserService currentUserService,
                               IBrowserService browserService,
                               IJSRuntime jSRuntime,
                               ContextMenuService contextMenuService,
                               DialogService dialogService,
                               IServiceScopeFactory serviceScopeFactory,
                               NotificationService notificationService,
                               ISettingsService settingsService,
                               NavigationManager navigationManager) : IRefreshableData,
                                                                      IAsyncDisposable
{
    private const int GridCols = 24;
    private const int GridRows = 24;
    private IEnumerable<Data> Items { get; set; } = [];
    private IEnumerable<string> SelectedClusterNames { get; set; } = [];
    private bool InEditing { get; set; }
    private bool ShowGrid { get; set; }
    private Data? SelectedItem { get; set; }
    private Models.Dashboard CurrentDashboard { get; set; } = new();
    private IEnumerable<ClusterSettings> ClustersSettings { get; set; } = [];
    private List<DashboardWidgetItem> WidgetItems { get; } = [];
    private string FileUploadId { get; } = Guid.NewGuid().ToString();

    private IDisposable? _navigationHandler;
    private Timer? _timer;
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };
    private bool _disposed;
    private int _widgetId;

    private record Data(string Name, int Id);

    protected override async Task OnInitializedAsync()
    {
        _navigationHandler = navigationManager.RegisterLocationChangingHandler(OnLocationChanging);
        ClustersSettings = [.. settingsService.GetEnabledClustersSettings()];
        await RefreshDataAsync();
    }

    private async ValueTask OnLocationChanging(LocationChangingContext context)
    {
        if (InEditing &&
            !await dialogService.ConfirmAsync(L["You have unsaved changes. Are you sure you want to leave?"],
                                              L["Unsaved changes"],
                                              true))
        {
            context.PreventNavigation();
        }
    }

    public async Task RefreshDataAsync()
    {
        if (_disposed) { return; }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        if (!await db.Dashboards.Where(a => a.UserId == currentUserService.UserId).AnyAsync())
        {
            await CreateDefaultAsync();
        }

        Items = await db.Dashboards
                        .Where(a => a.UserId == currentUserService.UserId)
                        .OrderBy(a => a.Name)
                        .Select(a => new Data(a.Name, a.Id))
                        .ToListAsync();

        if (SelectedItem == null || !Items.Any(a => a.Id == SelectedItem.Id))
        {
            SelectedItem = Items.FirstOrDefault()!;
        }

        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (_disposed) { return; }

        StopTimer();
        WidgetItems.Clear();

        if (SelectedItem == null)
        {
            CurrentDashboard = new() { UserId = currentUserService.UserId };
        }
        else
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            CurrentDashboard = (await db.Dashboards.Include(a => a.Widgets).FromIdAsync(SelectedItem.Id))!;
            StartTimer();
        }

        // Convert widgets to tile items
        foreach (var widget in CurrentDashboard.Widgets)
        {
            WidgetItems.Add(CreateWidgetItem(widget));
        }
        _widgetId = 0;

        await InvokeAsync(StateHasChanged);
    }

    private Task SetClusterNames() => RefreshDataAsync();

    private DashboardWidgetItem CreateWidgetItem(Widget widget)
        => DashboardWidgetItem.Create(widget, serviceScopeFactory, SelectedClusterNames, InEditing, this);

    #region Timer
    private void StartTimer()
    {
        StopTimer();
        if (CurrentDashboard?.RefreshInterval > 0)
        {
            _timer = new Timer(async _ =>
            {
                if (!_disposed) { await RefreshAllWidgetsAsync(); }
            }, null, CurrentDashboard.RefreshInterval * 1000, CurrentDashboard.RefreshInterval * 1000);
        }
    }

    private void StopTimer()
    {
        _timer?.Change(Timeout.Infinite, 0);
        _timer?.Dispose();
        _timer = null;
    }
    #endregion

    private async Task RefreshAllWidgetsAsync()
    {
        var tasks = WidgetItems
            .Where(a => a.Instance != null)
            .Select(item => item.Instance!.RefreshDataAsync());
        await Task.WhenAll(tasks);
    }

    private static async Task RefreshWidgetAsync(DashboardWidgetItem item)
    {
        if (item.Instance != null)
        {
            await item.Instance.RefreshDataAsync();
        }
    }

    #region CRUD Operations
    public async Task CreateDefaultAsync()
    {
        SelectedItem = null;
        await using var db = await dbContextFactory.CreateDbContextAsync();

        foreach (var item in Models.Dashboard.GetDefaults())
        {
            var newName = $"zzz_{item.Name}-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}";
            await db.Dashboards.Where(a => a.Name == item.Name)
                               .ExecuteUpdateAsync(a => a.SetProperty(b => b.Name, newName));

            item.UserId = currentUserService.UserId;
            await db.Dashboards.AddAsync(item);
            if (SelectedItem == null) { SelectedItem = new(item.Name, item.Id); }
        }

        await db.SaveChangesAsync();
    }

    public async Task CreateDefaultDashboardAsync()
    {
        await CreateDefaultAsync();
        await RefreshDataAsync();
    }

    private async Task SaveAsync()
    {
        if (_disposed) { return; }

        CurrentDashboard.UserId = currentUserService.UserId;

        await using var db = await dbContextFactory.CreateDbContextAsync();

        var origIds = await db.Widgets
                          .Where(a => a.Dashboard.Id == CurrentDashboard.Id)
                          .AsNoTracking()
                          .Select(a => a.Id)
                          .ToListAsync();

        var currentWidgetIds = CurrentDashboard.Widgets.Where(w => w.Id != 0).Select(w => w.Id).ToHashSet();
        var idsToRemove = origIds.Where(id => !currentWidgetIds.Contains(id)).ToList();

        if (idsToRemove.Count > 0)
        {
            await db.Widgets.Where(a => idsToRemove.Contains(a.Id)).ExecuteDeleteAsync();
        }

        foreach (var item in CurrentDashboard.Widgets.Where(w => w.Id <= 0))
        {
            item.Id = 0;
        }

        await db.AddOrUpdateAsync(CurrentDashboard);

        SelectedItem = new(CurrentDashboard.Name, CurrentDashboard.Id);
        await EndEditAsync();
    }

    private async Task DeleteAsync()
    {
        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Delete current dashboard"], true))
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            await db.Dashboards.DeleteAsync(CurrentDashboard.Id);

            SelectedItem = null!;
            await RefreshDataAsync();
        }
    }

    private async Task NewAsync()
    {
        StopTimer();
        CurrentDashboard = new();
        WidgetItems.Clear();
        InEditing = true;
        ShowGrid = true;
        await InvokeAsync(StateHasChanged);
    }

    private async Task CloneAsync()
    {
        CurrentDashboard.Name = string.Empty;
        CurrentDashboard.Id = 0;
        foreach (var item in CurrentDashboard.Widgets) { item.Id = _widgetId--; }
        await StartEditAsync();
    }
    #endregion

    #region Edit Mode
    private async Task StartEditAsync()
    {
        StopTimer();
        InEditing = true;
        ShowGrid = true;

        await RefreshDataAsync();
    }

    private async Task EndEditAsync()
    {
        InEditing = false;
        ShowGrid = false;
        await RefreshDataAsync();
    }
    #endregion

    #region Widget Operations
    private (int X, int Y) FindFirstFreePosition(int width, int height)
    {
        var occupied = new bool[GridCols, GridRows];

        foreach (var widget in CurrentDashboard.Widgets)
        {
            for (var x = widget.X; x < widget.X + widget.Width && x < GridCols; x++)
            {
                for (var y = widget.Y; y < widget.Y + widget.Height && y < GridRows; y++)
                {
                    occupied[x, y] = true;
                }
            }
        }

        for (var y = 0; y <= GridRows - height; y++)
        {
            for (var x = 0; x <= GridCols - width; x++)
            {
                if (CanPlace(occupied, x, y, width, height)) { return (x, y); }
            }
        }

        return (0, 0);
    }

    private static bool CanPlace(bool[,] occupied, int startX, int startY, int width, int height)
    {
        for (var x = startX; x < startX + width; x++)
        {
            for (var y = startY; y < startY + height; y++)
            {
                if (occupied[x, y]) { return false; }
            }
        }
        return true;
    }

    private async Task AddWidgetAsync(ModuleWidget widget)
    {
        var (x, y) = FindFirstFreePosition(widget.Width, widget.Height);

        var newWidget = new Widget
        {
            Title = $"{widget.Module.Name} - {widget.Name}",
            X = x,
            Y = y,
            Width = widget.Width,
            Height = widget.Height,
            ModuleWidgetType = widget.RenderInfo.Type.FullName!,
            Id = _widgetId--
        };

        CurrentDashboard.Widgets.Add(newWidget);
        WidgetItems.Add(CreateWidgetItem(newWidget));

        await InvokeAsync(StateHasChanged);
    }

    private async Task ConfigureWidgetAsync(DashboardWidgetItem item)
    {
        if (await item.ConfigureAsync(dialogService))
        {
            var index = WidgetItems.IndexOf(item);
            if (index >= 0) { WidgetItems[index] = CreateWidgetItem(item.Widget); }

        }
        await InvokeAsync(StateHasChanged);
    }

    private async Task CloneWidgetAsync(DashboardWidgetItem item)
    {
        var moduleWidget = item.Widget.GetModuleWidget(moduleService);
        if (moduleWidget != null)
        {
            await AddWidgetAsync(moduleWidget);
            var newWidget = CurrentDashboard.Widgets.Last();
            newWidget.Title = $"Copy of {item.Widget.Title}";
            newWidget.TitleCss = item.Widget.TitleCss;
            newWidget.BodyCss = item.Widget.BodyCss;
            newWidget.Height = item.Widget.Height;
            newWidget.Width = item.Widget.Width;
            newWidget.SettingsJson = item.Widget.SettingsJson;

            WidgetItems[^1] = CreateWidgetItem(newWidget);
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task RemoveWidgetAsync(DashboardWidgetItem item)
    {
        CurrentDashboard.Widgets.Remove(item.Widget);
        WidgetItems.Remove(item);
        await InvokeAsync(StateHasChanged);
    }
    #endregion

    #region Tile Layout Helpers
    private static Task OnTileChanged(DashboardWidgetItem item,
                                      int? col = null,
                                      int? row = null,
                                      int? colSpan = null,
                                      int? rowSpan = null)
    {
        if (col is not null)     { item.Widget.X = col.Value - 1; }
        if (row is not null)     { item.Widget.Y = row.Value - 1; }
        if (colSpan is not null) { item.Widget.Width = colSpan.Value; }
        if (rowSpan is not null) { item.Widget.Height = rowSpan.Value; }
        return Task.CompletedTask;
    }

    #endregion

    #region Import/Export
    private async Task UploadAsync()
        => await jSRuntime.InvokeVoidAsync("eval", $"document.getElementById('{FileUploadId}').click();");

    private async Task HandleFileSelectedAsync(InputFileChangeEventArgs args)
    {
        var file = args.File;
        if (file != null)
        {
            try
            {
                await using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);
                var item = JsonSerializer.Deserialize<Models.Dashboard>(await reader.ReadToEndAsync())!;

                await using var db = await dbContextFactory.CreateDbContextAsync();
                item.Name += await db.Dashboards.AnyAsync(a => a.Name == item.Name)
                            ? $"-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}"
                            : string.Empty;

                await db.Dashboards.AddAsync(item);
                await db.SaveChangesAsync();

                SelectedItem = new Data(item.Name, item.Id);
                await RefreshDataAsync();

                notificationService.Info(L["Dashboard '{0}' imported!", item.Name]);
            }
            catch (Exception ex)
            {
                notificationService.Error(L["Error import dashboard"], ex.Message);
            }
        }
    }

    private async Task ExportAsync()
    {
        await using var ms = new MemoryStream();
        await JsonSerializer.SerializeAsync(ms, CurrentDashboard, _jsonSerializerOptions);
        ms.Position = 0;
        await browserService.DownloadFileAsync($"dashboard-{CurrentDashboard.Name}.json", ms, MediaTypeNames.Application.Json);
    }
    #endregion

    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        StopTimer();
        _navigationHandler?.Dispose();
        await ValueTask.CompletedTask;
    }
}
