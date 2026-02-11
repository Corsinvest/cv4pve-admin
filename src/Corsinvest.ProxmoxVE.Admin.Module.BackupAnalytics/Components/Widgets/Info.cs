using Corsinvest.ProxmoxVE.Admin.Core.Components.Widgets;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Module.BackupAnalytics.Components.Widgets;

public class Info(IAdminService adminService,
                  ISettingsService settingsService,
                  EventNotificationService eventNotificationService) : WidgetInfoBase<object>
{
    protected override async Task OnInitializedAsync()
    {
        eventNotificationService.Subscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        await base.OnInitializedAsync();
    }

    private async Task HandleDataChangedNotificationAsync(DataChangedNotification notification)
    {
        await RefreshDataAsync();
        await InvokeAsync(StateHasChanged);
    }

    protected override async Task RefreshDataAsyncInt()
    {
        IsLoadingStats = true;
        IsLoadingAlerts = true;
        IsLoadingDistribution = true;
        Alerts = [];

        UpdateStatsAndDistribution([]);
        await InvokeAsync(StateHasChanged);

        var clusterNames = ClusterNames.Any()
                  ? ClusterNames
                  : settingsService.GetEnabledClustersSettings().Select(a => a.Name);

        var backups = new List<NodeStorageContent>();
        var vmsWithoutBackup = 0;

        // Phase 1: Load backups and VM ids incrementally per node
        foreach (var clusterName in clusterNames)
        {
            var clusterClient = adminService[clusterName];
            var client = await clusterClient.GetPveClientAsync();
            var resources = await clusterClient.CachedData.GetResourcesAsync(false);

            // Get all VM ids for this cluster
            var clusterVmIds = resources.Where(r => r.ResourceType == ClusterResourceType.Vm)
                                        .Select(r => r.VmId)
                                        .ToHashSet();

            // Get backups from all online nodes - update after each node
            foreach (var node in resources.Where(r => r.ResourceType == ClusterResourceType.Node && r.IsOnline))
            {
                backups.AddRange(await client.Nodes[node.Node].GetBackupsInAllStoragesAsync());

                // Update Stats incrementally
                UpdateStatsAndDistribution(backups);
                await InvokeAsync(StateHasChanged);
            }

            // Calculate VMs without backup for this cluster
            var clusterBackupVmIdsCount = backups.Where(b => clusterVmIds.Contains(b.VmId))
                                                 .Select(b => b.VmId)
                                                 .Distinct()
                                                 .Count();
            vmsWithoutBackup += clusterVmIds.Count - clusterBackupVmIdsCount;
        }

        IsLoadingStats = false;
        IsLoadingDistribution = false;

        // Build initial alerts (without unprotected disks)
        var count = backups.Count;
        var ver = backups.Count(a => a.Verified);
        var enc = backups.Count(a => a.Encrypted);
        var unver = count - ver;
        var unenc = count - enc;
        var now = DateTime.UtcNow;
        var moreThan90d = backups.Count(a => (now - a.CreationDate).TotalDays >= 90);

        var alerts = new List<AlertItem>();
        if (unver > 0) { alerts.Add(new AlertItem("warning", "rz-color-warning", L["{0} unverified backups", unver])); }
        if (unenc > 0) { alerts.Add(new AlertItem("lock_open", "rz-color-info", L["{0} unencrypted backups", unenc])); }
        if (vmsWithoutBackup > 0) { alerts.Add(new AlertItem("error", "rz-color-danger", L["{0} VMs without backup", vmsWithoutBackup])); }
        if (moreThan90d > 0) { alerts.Add(new AlertItem("event_busy", "rz-color-base-500", L["{0} backups older than 90 days", moreThan90d])); }
        Alerts = alerts;
        await InvokeAsync(StateHasChanged);

        // Phase 2: Load unprotected disks (slow, requires per-VM API calls)
        var vmsWithUnprotectedDisks = 0;
        foreach (var clusterName in clusterNames)
        {
            var clusterClient = adminService[clusterName];
            var client = await clusterClient.GetPveClientAsync();

            foreach (var vm in await client.GetVmsAsync())
            {
                VmConfig config = vm.VmType switch
                {
                    VmType.Qemu => await client.Nodes[vm.Node].Qemu[vm.VmId].Config.GetAsync(),
                    VmType.Lxc => await client.Nodes[vm.Node].Lxc[vm.VmId].Config.GetAsync(),
                    _ => null!
                };

                if (config?.Disks.Any(a => !a.Backup) == true)
                {
                    vmsWithUnprotectedDisks++;
                }
            }
        }

        // Update alerts with unprotected disks
        if (vmsWithUnprotectedDisks > 0)
        {
            alerts = [.. Alerts];
            // Insert before "backups older than 90 days" if present
            var insertIndex = alerts.FindIndex(a => a.Icon == "event_busy");
            if (insertIndex >= 0)
            {
                alerts.Insert(insertIndex, new AlertItem("disc_full", "rz-color-warning", L["{0} VMs with unprotected disks", vmsWithUnprotectedDisks]));
            }
            else
            {
                alerts.Add(new AlertItem("disc_full", "rz-color-warning", L["{0} VMs with unprotected disks", vmsWithUnprotectedDisks]));
            }
            Alerts = alerts;
        }
        IsLoadingAlerts = false;
        await InvokeAsync(StateHasChanged);
    }

    private void UpdateStatsAndDistribution(List<NodeStorageContent> backups)
    {
        var count = backups.Count;
        var ver = backups.Count(a => a.Verified);
        var enc = backups.Count(a => a.Encrypted);
        var prot = backups.Count(a => a.Protected);

        Stats =
        [
            new (L["TOTAL"], count.ToString(), L["backups"], Colors.Primary),
            new (L["VERIFIED"], ver.ToString(), $"{(count == 0 ? 0 : (ver * 100 / count))}%", Colors.Success),
            new (L["ENCRYPTED"], enc.ToString(), $"{(count == 0 ? 0 : (enc * 100 / count))}%", Colors.Warning),
            new (L["PROTECTED"], prot.ToString(), $"{(count == 0 ? 0 : (prot * 100 / count))}%", Colors.Info)
        ];

        var now = DateTime.UtcNow;
        var lessThan7d = backups.Count(a => (now - a.CreationDate).TotalDays < 7);
        var between7And30d = backups.Count(a => (now - a.CreationDate).TotalDays >= 7 && (now - a.CreationDate).TotalDays < 30);
        var between30And90d = backups.Count(a => (now - a.CreationDate).TotalDays >= 30 && (now - a.CreationDate).TotalDays < 90);
        var moreThan90d = backups.Count(a => (now - a.CreationDate).TotalDays >= 90);

        var maxAge = Math.Max(1, Math.Max(lessThan7d, Math.Max(between7And30d, Math.Max(between30And90d, moreThan90d))));
        AgeDistribution =
        [
            new ("< 7d", lessThan7d, lessThan7d * 100 / maxAge, ProgressBarStyle.Success),
            new ("7-30d", between7And30d, between7And30d * 100 / maxAge, ProgressBarStyle.Info),
            new ("30-90d", between30And90d, between30And90d * 100 / maxAge, ProgressBarStyle.Warning),
            new ("> 90d", moreThan90d, moreThan90d * 100 / maxAge, ProgressBarStyle.Danger)
        ];
    }

    public override void Dispose()
    {
        eventNotificationService.Unsubscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        base.Dispose();
    }
}
