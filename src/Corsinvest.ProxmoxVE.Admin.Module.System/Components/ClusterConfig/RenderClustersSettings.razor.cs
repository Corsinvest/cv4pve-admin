/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.System.Components.ClusterConfig;

public partial class RenderClustersSettings(ISettingsService settingsService,
                                            IAdminService adminService,
                                            DialogService dialogService,
                                            NotificationService notificationService,
                                            IBackgroundJobService backgroundJobService,
                                            IModuleService moduleService,
                                            IServiceScopeFactory scopeFactory,
                                            IDbContextFactory<ModuleDbContext> dbContextFactory,
                                            NavigationManager navigationManager)
{
    [SupplyParameterFromQuery] public bool? Create { get; set; }

    private ClustersSettings ClustersSettings { get; set; } = [];
    private IList<ClusterSettings> SelectedItems { get; set; } = [];
    private RadzenDataGrid<ClusterSettings> DataGridRef { get; set; } = default!;
    private bool _validColumnClick;

    protected override void OnInitialized() => ClustersSettings = settingsService.GetClustersSettings();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender
            && Create is true
            && !settingsService.GetEnabledClustersSettings().Any())
        {
            await AddAsync();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task RowSelectAsync(ClusterSettings item)
    {
        if (_validColumnClick) { await ShowEditorAsync(item, false); }
    }

    private void CellClick(DataGridCellMouseEventArgs<ClusterSettings> e)
        => _validColumnClick = new[] { nameof(ClusterSettings.Name), nameof(ClusterSettings.PveName) }.Contains(e.Column!.Property);

    private async Task DeleteAsync()
    {
        var cluster = SelectedItems[0];
        if (await dialogService.ConfirmAsync(L["Delete the cluster {0}? Its settings in every module and the data the modules collected for it (scans, results, history, UPS devices, tenants) are deleted too.", cluster.Name],
                                             L["Delete cluster"],
                                             true))
        {
            ClustersSettings.Remove(cluster);
            await settingsService.SetAsync(ClustersSettings);
            await DeleteClusterDataAsync(cluster.Name);
            await RefreshJobsAsync();
            await DataGridRef.Reload();
            navigationManager.ForceReload();
        }
    }

    private async Task DeleteClusterDataAsync(string clusterName)
    {
        using var scope = scopeFactory.CreateScope();
        foreach (var module in moduleService.Modules)
        {
            var maintenance = module.GetMaintenance(scope);
            if (maintenance == null) { continue; }

            try
            {
                await maintenance.DeleteClusterDataAsync(clusterName, CancellationToken.None);
            }
            catch (Exception ex)
            {
                notificationService.Error(L["{0}: data of the cluster not deleted: {1}", module.Name, ex.Message]);
            }
        }

        // Module settings are saved per cluster as "{settings type}-{cluster}": type names have no dash
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var settings = (await db.Settings.Where(a => a.Context == "System").ToListAsync())
                        .Where(a => a.Key.Contains('-') && a.Key[(a.Key.IndexOf('-') + 1)..] == clusterName)
                        .ToList();
        db.Settings.RemoveRange(settings);
        await db.SaveChangesAsync();
        await settingsService.ClearCacheAsync();
    }

    // The modules schedule the jobs of the enabled clusters again, then the jobs of the others are removed
    private async Task RefreshJobsAsync()
    {
        using var scope = scopeFactory.CreateScope();
        foreach (var module in moduleService.Modules) { await module.RefreshSettingsEventAsync(scope); }

        backgroundJobService.RemoveForMissingClusters(ClustersSettings.Where(a => a.Enabled).Select(a => a.Name));
    }

    private Task AddAsync() => ShowEditorAsync(new(), true);

    private async Task ShowEditorAsync(ClusterSettings item, bool isNew)
    {
        if (await dialogService.OpenSideEditAsync<ClusterSettingsDialog>(isNew
                                                                            ? L["New Cluster Settings"]
                                                                            : L["Edit {0}", item.Name],
                                                                         isNew
                                                                            ? EditDialogMode.Create
                                                                            : EditDialogMode.Edit,
                                                                         item,
                                                                         OnSubmitingDialog) != null)
        {
            if (isNew)
            {
                ClustersSettings.Add(item);
                await DataGridRef.Reload();
            }
            await settingsService.SetAsync(ClustersSettings);
            await RefreshJobsAsync();

            if (item.Enabled) { await adminService[item.Name].CachedData.ClearCacheAsync(); }

            navigationManager.ForceReload();
        }
    }

    private async Task<bool> OnSubmitingDialog(object model, bool isNew)
    {
        var clusterSettings = (ClusterSettings)model;
        var valid = !clusterSettings.Enabled
                    || await PveAdminUIHelper.PopulateClusterSettingsAsync(adminService, clusterSettings, dialogService, notificationService, L);

        if (valid && clusterSettings.Enabled && clusterSettings.SshCredential.IsConfigured)
        {
            valid = await PveAdminUIHelper.TestSshAsync(adminService, clusterSettings, dialogService, notificationService, L);
        }

        if (valid)
        {
            var detail = string.Empty;

            if (ClustersSettings.Any(a => a.PveName != clusterSettings.PveName && a.Name == clusterSettings.Name))
            {
                valid = false;
                detail = "Name not unique!";
            }

            if (!valid)
            {
                notificationService.Notify(new()
                {
                    Severity = NotificationSeverity.Error,
                    Summary = L["Error"],
                    Detail = L[detail],
                    Duration = 20000
                });
            }
        }

        return valid;
    }
}
