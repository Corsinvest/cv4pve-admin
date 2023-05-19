/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.UI;
using Corsinvest.ProxmoxVE.Admin.Core.UI.Dialogs;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;
using Microsoft.JSInterop;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.Layout;

public partial class SelectCluster : AHComponentBase, IUIAppBarItem
{
    [Inject] private IPveClientService PveClientService { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    public Type Render { get; } = typeof(SelectCluster);
    private string CurrentClusterName { get; set; } = default!;
    private bool _refresh;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (PveClientService.GetClusters().Any())
            {
                var clusterName = await PveClientService.GetCurrentClusterName();
                if (string.IsNullOrEmpty(clusterName))
                {
                    clusterName = PveClientService.GetClusters().ToArray()[0].Name;
                    await PveClientService.SetCurrentClusterName(clusterName);
                }

                _refresh = false;
                await ValueChanged(clusterName);
                _refresh = true;
            }
            else
            {
                ShowDialogFastConfig();
            }
        }
    }

    private async Task ValueChanged(string value)
    {
        if (await IsValid(value))
        {
            await PveClientService.SetCurrentClusterName(value);
            CurrentClusterName = value;

            if (_refresh)
            {
                NavigationManager.NavigateTo(NavigationManager.Uri, true);
            }
        }
        else
        {
            await PveClientService.SetCurrentClusterName(string.Empty);
            if (PveClientService.GetClusters().Count() == 1)
            {
                ShowDialogFastConfig();
            }
            else
            {

            }
        }
        StateHasChanged();
    }

    private async Task<bool> IsValid(string clusterName)
    {
        try
        {
            var ret = false;
            if (!string.IsNullOrEmpty(clusterName))
            {
                var client = await PveClientService.GetClient(clusterName);
                ret = client != null && await PveAdminHelper.CheckIsValidVersion(client);
            }

            return ret;
        }
        catch // (Exception ex)
        {
            return false;
        }
    }

    public async Task OpenUrl(ClusterOptions item) => await JSRuntime.InvokeVoidAsync("open", PveAdminHelper.GetPveUrl(item), "_blank");

    private void ShowDialogFastConfig()
        => DialogService.Show<DialogFastConfig>(L["Fast Configuration"],
                                                new DialogOptions
                                                {
                                                    CloseButton = false,
                                                    DisableBackdropClick = true,
                                                    MaxWidth = MaxWidth.Medium,
                                                    FullWidth = true,
                                                });
}