using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Vm;

public partial class Manager : IRefreshableData, IDisposable, IClusterName
{
    [EditorRequired, Parameter] public IClusterResourceVm Vm { get; set; } = default!;
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;

    private RadzenTabs RadzenTabsRef { get; set; } = default!;
    private Summary SummaryRef { get; set; } = default!;
    private Charts ChartsRef { get; set; } = default!;
    private Snapshot.Manager SnapshotManagerRef { get; set; } = default!;
    private Nodes.Replication ReplicationRef { get; set; } = default!;
    private Common.Tasks TasksRef { get; set; } = default!;
    private bool CanSnapshots { get; set; }
    private bool CanReplication { get; set; }
    private bool CanBackups { get; set; }
    private bool CanAudit { get; set; }
    private bool CanSnapshotRallback { get; set; }
    private bool CanBackupRestore { get; set; }
    private bool CanBackupRestoreFile { get; set; }
    private bool CanReplicationScheduleNow { get; set; }
    private bool CanConsole { get; set; }
    private bool CanPowerManagement { get; set; }
    private TabContent CurrentTabContent => (TabContent)RadzenTabsRef.SelectedTab!.Attributes!["tab-content"];

    private Timer? _timer;
    private CancellationTokenSource? _cts;

    private enum TabContent
    {
        Summary,
        Config,
        QemuGeuestInfo,
        Charts,
        Tasks,
        Replication,
        Backup,
        Snapshot
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
        CanConsole = await PermissionService.HasVmAsync(ClusterName, ClusterPermissions.Vm.Console, Vm.VmId);
        CanPowerManagement = await PermissionService.HasVmAsync(ClusterName, ClusterPermissions.Vm.PowerManagement, Vm.VmId);
        CanSnapshots = await PermissionService.HasVmAsync(ClusterName, ClusterPermissions.Vm.Snapshot, Vm.VmId);
        CanReplication = await PermissionService.HasVmAsync(ClusterName, ClusterPermissions.Vm.Replication, Vm.VmId);
        CanBackups = await PermissionService.HasVmAsync(ClusterName, ClusterPermissions.Vm.Backup, Vm.VmId);
        CanAudit = await PermissionService.HasVmAsync(ClusterName, ClusterPermissions.Vm.Audit, Vm.VmId);
    }

    private async Task SelectedIndexChangedAsync()
    {
        switch (CurrentTabContent)
        {
            case TabContent.Summary: break;
            case TabContent.Config: break;
            case TabContent.QemuGeuestInfo: break;
            case TabContent.Charts: break;
            case TabContent.Tasks: break;
            case TabContent.Replication:
                CanReplicationScheduleNow = await PermissionService.HasVmAsync(ClusterName, ClusterPermissions.Vm.ReplicationScheduleNow, Vm.VmId);
                break;

            case TabContent.Backup:
                CanBackupRestore = await PermissionService.HasVmAsync(ClusterName, ClusterPermissions.Vm.BackupRestore, Vm.VmId);
                CanBackupRestoreFile = await PermissionService.HasVmAsync(ClusterName, ClusterPermissions.Vm.BackupRestoreFile, Vm.VmId);
                break;

            case TabContent.Snapshot:
                CanSnapshotRallback = await PermissionService.HasVmAsync(ClusterName, ClusterPermissions.Vm.SnapshotRallback, Vm.VmId);
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
                TabContent.Config => IRefreshableData.Dummy,
                TabContent.QemuGeuestInfo => IRefreshableData.Dummy,
                TabContent.Charts => ChartsRef,
                TabContent.Tasks => TasksRef,
                TabContent.Replication => ReplicationRef,
                TabContent.Backup => IRefreshableData.Dummy,
                TabContent.Snapshot => IRefreshableData.Dummy, //SnapshotManagerRef,
                _ => IRefreshableData.Dummy
            }).RefreshDataAsync();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to refresh VM {VmId} tab {TabContent} in cluster {ClusterName}",
                Vm.VmId, CurrentTabContent, ClusterName);
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
