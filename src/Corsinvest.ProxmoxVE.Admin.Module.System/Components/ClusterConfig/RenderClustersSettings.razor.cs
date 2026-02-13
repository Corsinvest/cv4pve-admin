/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Web;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Components.ClusterConfig;

public partial class RenderClustersSettings(ISettingsService settingsService,
                                            IAdminService adminService,
                                            DialogService dialogService,
                                            DialogService sialogService,
                                            NotificationService notificationService,
                                            NavigationManager navigationManager)
{
    private ClustersSettings ClustersSettings { get; set; } = [];
    private IList<ClusterSettings> SelectedItems { get; set; } = [];
    private RadzenDataGrid<ClusterSettings> DataGridRef { get; set; } = default!;
    private bool _validColumnClick;

    protected override void OnInitialized() => ClustersSettings = settingsService.GetClustersSettings();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var keys = (HttpUtility.ParseQueryString(new Uri(navigationManager.Uri).Query).AllKeys ?? [])
                                   .Where(a => !string.IsNullOrEmpty(a));

            if (keys.Any(a => a!.Equals("new", StringComparison.CurrentCultureIgnoreCase)))
            {
                await AddAsync();
            }
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
        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Delete selected row"], true))
        {
            ClustersSettings.Remove(SelectedItems[0]);
            await settingsService.SetAsync(ClustersSettings);
            await DataGridRef.Reload();
        }
    }

    private async Task AddAsync() => await ShowEditorAsync(new(), true);

    private async Task ShowEditorAsync(ClusterSettings item, bool isNew)
    {
        var title = isNew
                        ? L["New Cluster Settings"]
                        : L["Edit {0}", item.Name];

        if (await sialogService.OpenSideEditAsync<ClusterSettingsDialog>(title, isNew, item, OnSubmitingDialog) != null)
        {
            if (isNew)
            {
                ClustersSettings.Add(item);
                await DataGridRef.Reload();
            }
            await settingsService.SetAsync(ClustersSettings);
            await adminService[item.Name].CachedData.ClearCacheAsync();

            navigationManager.NavigateTo(new Uri(navigationManager.Uri).LocalPath, forceLoad: true);
        }
    }

    private async Task<bool> OnSubmitingDialog(object model, bool isNew)
    {
        var clusterSettings = (ClusterSettings)model;
        var valid = !clusterSettings.Enabled
                    || await PveAdminUIHelper.PopulateClusterSettingsAsync(adminService, clusterSettings, sialogService, notificationService, L);

        if (valid)
        {
            var detail = string.Empty;

            if (string.IsNullOrEmpty(clusterSettings.Name) || string.IsNullOrWhiteSpace(clusterSettings.Name))
            {
                valid = false;
                detail = "Name not valid!";
            }
            else if (ClustersSettings.Any(a => a.PveName != clusterSettings.PveName && a.Name == clusterSettings.Name))
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
