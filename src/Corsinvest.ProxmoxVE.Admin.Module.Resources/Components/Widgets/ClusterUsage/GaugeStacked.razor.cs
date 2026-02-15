/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components.Widgets.ClusterUsage;

public partial class GaugeStacked(IAdminService adminService) : IModuleWidget<object>, IDisposable
{
    [Parameter] public object Settings { get; set; } = default!;
    [Parameter] public EventCallback<object> SettingsChanged { get; set; }
    [Parameter] public IEnumerable<string> ClusterNames { get; set; } = [];
    [Parameter] public bool InEditing { get; set; }

    private IEnumerable<ResourceUsage> Items { get; set; } = [];
    private bool Initalized { get; set; }

    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    protected override async Task OnInitializedAsync()
    {
        await RefreshDataAsync();
        Initalized = true;
    }

    public async Task RefreshDataAsync()
    {
        if (!await _refreshLock.WaitAsync(0)) { return; }
        try
        {
            await RefreshDataAsyncInt();
        }
        finally
        {
            try { _refreshLock?.Release(); } catch (ObjectDisposedException) { }
        }
    }

    private async Task RefreshDataAsyncInt()
    {
        var items = new List<ClusterResource>();
        foreach (var clusterClient in adminService.Where(a => ClusterNames.Contains(a.Settings.Name), ClusterNames.Any()))
        {
            items.AddRange(await clusterClient.CachedData.GetResourcesAsync(false));
        }
        Items = ResourceUsage.Get(items, L);
    }

    public void Dispose()
    {
        _refreshLock?.Dispose();
        GC.SuppressFinalize(this);
    }
}
