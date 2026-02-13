/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Microsoft.JSInterop;
using OpenLayers.Blazor;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components.Widgets.Maps;

public partial class Render(IAdminService adminService,
                            IJSRuntime jsRuntime,
                            DialogService DialogService) : IModuleWidget<object>, IDisposable
{
    [Parameter] public object Settings { get; set; } = default!;
    [Parameter] public EventCallback<object> SettingsChanged { get; set; }
    [Parameter] public IEnumerable<string> ClusterNames { get; set; } = [];
    [Parameter] public bool InEditing { get; set; }

    private OpenStreetMap RefMap { get; set; } = default!;
    private bool Initalized { get; set; }
    private IEnumerable<Data> Items { get; set; } = [];
    private IEnumerable<Detail> Details { get; set; } = [];
    private ICollection<ResourceUsage> DataUsages { get; set; } = [];

    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private record Data(string ClusterName, Coordinate Coordinate, PinColor PinColor);
    private record Detail(string Type, string Status, int Count);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await jsRuntime.InvokeVoidAsync("eval",
                                            """
                                                 if (!document.getElementById('openlayers-css')) {
                                                     var link = document.createElement('link');
                                                     link.rel = 'stylesheet';
                                                     link.href = 'lib/openlayers/ol.css';
                                                     link.id = 'openlayers-css';
                                                     document.head.appendChild(link);
                                                 }

                                                 if (!document.getElementById('openlayers-js'))
                                                 {
                                                     var script = document.createElement('script');
                                                     script.src = 'lib/openlayers/dist/ol.min.js';
                                                     script.id = 'openlayers-js';
                                                     document.head.appendChild(script);
                                                 }
                                                 """);

            //await RefreshDataAsync();
            //if (RefMap != null) { await RefMap.CenterToCurrentGeoLocation(); }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        Items = adminService.Select(a => new Data(a.Settings.Name,
                                                  new(a.Settings.Longitude, a.Settings.Latitude),
                                                  PinColor.Blue));

        await RefreshDataAsync();

        Initalized = true;
    }

    private async Task OnLayerAdded(Layer layer) => await RefMap.CenterToCurrentGeoLocation();

    //private async Task OnMarkerClickAsync(string clusterName)
    //{
    //    var resources = await adminService[clusterName].CachedData.GetResourcesAsync(false);

    //    DataUsages = ResourceUsage.Get(resources, L);
    //    DataUsages.Add(await ResourceUsage.GetSnapshots(resources, L, adminService[clusterName]));

    //    Details = [.. resources.Where(a => a.ResourceType is ClusterResourceType.Vm or ClusterResourceType.Node)
    //                           .OrderBy(a => a.ResourceType)
    //                           .ThenBy(a => a.Type)
    //                           .GroupBy(a => new { a.Type, a.Status })
    //                           .Select(a=> new Detail(FormatValue(a.Key.Type),
    //                                                  FormatValue(a.Key.Status),
    //                                                  a.Count()))];
    //}

    private static string FormatValue(string value)
        => value switch
        {
            "running" or "online" or "available" => "🟢 ",
            "stopped" => "🔴 ",
            "unknown" => "❓ ",
            "node" => "🖧 ",
            "qemu" => "🖥️ ",
            "lxc" => "📦 ",
            _ => string.Empty
        } + value;

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

    private async Task RefreshDataAsyncInt()
    {
        var items = new List<Data>();

        foreach (var item in Items)
        {
            var resources = await adminService[item.ClusterName].CachedData.GetResourcesAsync(false);
            var dataUsages = ResourceUsage.Get(resources, L);

            items.Add(new Data(item.ClusterName,
                                    item.Coordinate,
                                    dataUsages.Any(a => a.Usage > 80)
                                    ? PinColor.Red
                                    : PinColor.Green));
        }

        Items = items;
    }

    public void Dispose()
    {
        _refreshLock?.Dispose();
        GC.SuppressFinalize(this);
    }
}
