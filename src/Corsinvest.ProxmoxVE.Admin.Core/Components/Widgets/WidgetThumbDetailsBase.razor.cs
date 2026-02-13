/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;

public abstract partial class WidgetThumbDetailsBase<TWidgetSettings>(IAdminService adminService,
                                                                      ISettingsService settingsService) : IModuleWidget<TWidgetSettings>, IDisposable
{
    [Parameter] public TWidgetSettings Settings { get; set; } = default!;
    [Parameter] public EventCallback<TWidgetSettings> SettingsChanged { get; set; }
    [Parameter] public IEnumerable<string> ClusterNames { get; set; } = [];
    [Parameter] public bool InEditing { get; set; }

    protected IEnumerable<Data> Items { get; set; } = [];
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    protected bool ShowIcon { get; set; } = true;

    protected string Message { get; set; } = default!;

    protected record Data(string Action, int Count);

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    protected IEnumerable<string> GetClusterNames<TModule, TSettings>()
        where TModule : ModuleBase
        where TSettings : IEnabled, IClusterName
    {
        var clusterNames = ClusterNames.Any()
                            ? ClusterNames
                            : adminService.Select(a => a.Settings.Name);

        return [.. clusterNames.Select(settingsService.GetForModule<TModule, TSettings>)
                             .Where(a => a.Enabled)
                             .Select(a => a.ClusterName)];
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

    protected abstract Task RefreshDataAsyncInt();

    public void Dispose()
    {
        _refreshLock?.Dispose();
        GC.SuppressFinalize(this);
    }
}
