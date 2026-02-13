/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Nodes;

public partial class Manager : IRefreshableData, IDisposable, IClusterName
{
    [EditorRequired, Parameter] public IClusterResourceNode Node { get; set; } = default!;
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;

    private RadzenTabs RadzenTabsRef { get; set; } = default!;
    private Summary SummaryRef { get; set; } = default!;
    private Charts ChartsRef { get; set; } = default!;
    private Replication ReplicationRef { get; set; } = default!;
    private Common.Tasks TasksRef { get; set; } = default!;
    private TabContent CurrentTabContent => (TabContent)RadzenTabsRef.SelectedTab!.Attributes!["tab-content"];
    private bool CanAudit { get; set; }
    private bool CanReplication { get; set; }
    private bool CanReplicationScheduleNow { get; set; }
    private bool CanConsole { get; set; }
    private bool CanPowerManagement { get; set; }

    private Timer? _timer;
    private CancellationTokenSource? _cts;

    private enum TabContent
    {
        Summary,
        Charts,
        Replication,
        Disks,
        Tasks
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
            }, null, 0, 2000);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        CanConsole = await PermissionService.HasNodeAsync(ClusterName, ClusterPermissions.Node.Console, Node.Node);
        CanPowerManagement = await PermissionService.HasNodeAsync(ClusterName, ClusterPermissions.Node.PowerManagement, Node.Node);
        CanAudit = await PermissionService.HasNodeAsync(ClusterName, ClusterPermissions.Node.Audit, Node.Node);
    }

    private async Task SelectedIndexChangedAsync()
    {
        switch (CurrentTabContent)
        {
            case TabContent.Summary: break;
            case TabContent.Disks: break;
            case TabContent.Charts: break;
            case TabContent.Tasks: break;
            case TabContent.Replication:
                CanReplicationScheduleNow = await PermissionService.HasNodeAsync(ClusterName, ClusterPermissions.Vm.ReplicationScheduleNow, Node.Node);
                break;
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
                TabContent.Tasks => TasksRef,
                TabContent.Replication => ReplicationRef,
                TabContent.Disks => IRefreshableData.Dummy,
                _ => IRefreshableData.Dummy
            }).RefreshDataAsync();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to refresh node {Node} tab {TabContent} in cluster {ClusterName}",
                Node.Node, CurrentTabContent, ClusterName);
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
