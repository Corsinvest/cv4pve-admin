using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Storage;

public partial class Manager : IRefreshableData, IDisposable, IClusterName
{
    [EditorRequired, Parameter] public IClusterResourceStorage Storage { get; set; } = default!;
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;

    private RadzenTabs RadzenTabsRef { get; set; } = default!;
    private Summary SummaryRef { get; set; } = default!;
    private Charts ChartsRef { get; set; } = default!;
    //private string Path => PermissionHelper.(Storage.Storage);
    private TabContent CurrentTabContent => (TabContent)RadzenTabsRef.SelectedTab!.Attributes!["tab-content"];
    private Timer? _timer;
    private CancellationTokenSource? _cts;

    private enum TabContent
    {
        Summary,
        Charts
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            _cts = new CancellationTokenSource();
            _timer = new Timer(async _ =>
            {
                if (!_cts.Token.IsCancellationRequested)
                {
                    await RefreshDataAsync();
                }
            }, null, 0, 30000);
        }
    }

    public async Task RefreshDataAsync()
    {
        try
        {
            await (CurrentTabContent switch
            {
                TabContent.Summary => SummaryRef,
                TabContent.Charts => ChartsRef,
                _ => IRefreshableData.Dummy
            }).RefreshDataAsync();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to refresh storage {Storage} tab {TabContent} in cluster {ClusterName}",
                Storage.Storage, CurrentTabContent, ClusterName);
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _timer?.Change(Timeout.Infinite, 0);
        _timer?.Dispose();
        _timer = null;
        _cts?.Dispose();
        _cts = null;
    }
}
