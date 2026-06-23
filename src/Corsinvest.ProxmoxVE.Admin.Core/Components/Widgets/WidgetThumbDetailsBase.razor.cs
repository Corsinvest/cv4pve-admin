/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;

public abstract partial class WidgetThumbDetailsBase<TWidgetSettings>(IAdminService adminService,
                                                                      ISettingsService settingsService) : IModuleWidget<TWidgetSettings>, IDisposable
{
    protected IAdminService AdminService => adminService;
    protected ISettingsService SettingsService => settingsService;

    [Parameter] public TWidgetSettings Settings { get; set; } = default!;
    [Parameter] public EventCallback<TWidgetSettings> SettingsChanged { get; set; }
    [Parameter] public IEnumerable<string> ClusterNames { get; set; } = [];
    [Parameter] public bool InEditing { get; set; }

    /// <summary>Number shown in the badge. The consumer sets this directly; no need to fake an Items entry.</summary>
    protected int Count { get; set; }

    /// <summary>Optional details rendered in the tooltip. Leave empty when the badge count alone is enough.</summary>
    protected IEnumerable<Data> Items { get; set; } = [];

    /// <summary>Free-form context line shown under the icon (e.g. "Last 7 days", "2 nodes online").</summary>
    protected string Message { get; set; } = default!;

    /// <summary>Explicit widget state. Drives the icon and whether the badge is shown.</summary>
    protected WidgetState Status { get; set; } = WidgetState.Ok;

    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private bool _disposed;

    protected record Data(string Text, int Count, string? Url = null);

    /// <summary>Visual state of the widget.</summary>
    protected enum WidgetState
    {
        /// <summary>Everything fine — thumb up, no badge.</summary>
        Ok,
        /// <summary>Issues found — thumb down + badge with Count.</summary>
        Issues,
        /// <summary>Module/data not available — neutral icon, no badge.</summary>
        NotConfigured
    }

    protected override Task OnInitializedAsync() => RefreshDataAsync();

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

    public void Dispose()
    {
        _disposed = true;
        _refreshLock?.Dispose();
        GC.SuppressFinalize(this);
    }
}
