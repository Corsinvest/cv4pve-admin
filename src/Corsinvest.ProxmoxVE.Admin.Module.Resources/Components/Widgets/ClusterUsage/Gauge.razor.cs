/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components.Widgets.ClusterUsage;

public partial class Gauge(IAdminService adminService) : IModuleWidget<object>, IDisposable
{
    [Parameter] public object Settings { get; set; } = default!;
    [Parameter] public EventCallback<object> SettingsChanged { get; set; }
    [Parameter] public IEnumerable<string> ClusterNames { get; set; } = [];
    [Parameter] public bool InEditing { get; set; }

    private IEnumerable<ResourceUsage> Items { get; set; } = [];
    private bool Initalized { get; set; }

    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private bool _disposed;

    protected override async Task OnInitializedAsync()
    {
        await RefreshDataAsync();
        Initalized = true;
    }

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

    private async Task RefreshDataAsyncInt()
    {
        var items = new List<ClusterResource>();
        foreach (var clusterClient in adminService.Where(a => ClusterNames.Contains(a.Settings.Name), ClusterNames.Any()))
        {
            items.AddRange(await clusterClient.CachedData.GetResourcesAsync(false));
        }
        var usages = ResourceUsage.Get(items, L);
        foreach (var clusterClient in adminService.Where(a => ClusterNames.Contains(a.Settings.Name), ClusterNames.Any()))
        {
            usages.Add(await ResourceUsage.GetSnapshots(items, L, clusterClient));
        }
        Items = usages;
    }

    public void Dispose()
    {
        _disposed = true;
        _refreshLock?.Dispose();
        GC.SuppressFinalize(this);
    }
}
