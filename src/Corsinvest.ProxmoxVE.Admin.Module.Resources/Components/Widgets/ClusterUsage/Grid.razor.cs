/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components.Widgets.ClusterUsage;

public partial class Grid(IAdminService adminService) : IModuleWidget<object>, IDisposable
{
    [Parameter] public object Settings { get; set; } = default!;
    [Parameter] public EventCallback<object> SettingsChanged { get; set; }
    [Parameter] public IEnumerable<string> ClusterNames { get; set; } = [];
    [Parameter] public bool InEditing { get; set; }

    private IList<Data> Items { get; set; } = [];

    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private bool _disposed;

    private class Data : IClusterName, IDescription
    {
        public string ClusterName { get; set; } = default!;
        public string PveName { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string Link { get; set; } = default!;
        public double CpuUsage { get; set; }
        public string CpuInfo { get; set; } = default!;
        public double MemoryUsage { get; set; }
        public string MemoryInfo { get; set; } = default!;
        public double DiskUsage { get; set; }
        public string DiskInfo { get; set; } = default!;
        public ClusterType Type { get; set; }
        public string Icon { get; set; } = default!;
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

    private async Task RefreshDataAsyncInt()
    {
        foreach (var clusterClient in adminService.Where(a => ClusterNames.Contains(a.Settings.Name), ClusterNames.Any()))
        {
            var usage = ResourceUsage.Get(await clusterClient.CachedData.GetResourcesAsync(false), L);

            var row = Items.FromClusterName(clusterClient.Settings.Name);
            if (row == null)
            {
                row = new()
                {
                    ClusterName = clusterClient.Settings.Name,
                    PveName = clusterClient.Settings.PveName,
                    Type = clusterClient.Settings.Type,
                    Icon = clusterClient.Settings.Icon,
                    Description = clusterClient.Settings.Description
                };
                Items.Add(row);
            }
            row.CpuUsage = usage[0].Usage / 100.0;
            row.CpuInfo = usage[0].Info;
            row.MemoryUsage = usage[1].Usage / 100.0;
            row.MemoryInfo = usage[2].Info;
            row.DiskUsage = usage[2].Usage / 100.0;
            row.DiskInfo = usage[2].Info;
            row.Link = (await clusterClient.GetPveClientAsync()).BaseAddress;
        }

        Items = [.. Items];
    }

    public void Dispose()
    {
        _disposed = true;
        _refreshLock?.Dispose();
        GC.SuppressFinalize(this);
    }
}
