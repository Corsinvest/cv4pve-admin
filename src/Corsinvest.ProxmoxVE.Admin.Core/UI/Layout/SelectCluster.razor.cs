/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.UI;
using Corsinvest.ProxmoxVE.Admin.Core.UI.Dialogs;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.Layout;

public partial class SelectCluster : AHComponentBase, IUIAppBarItem
{
    [Inject] private IPveClientService PveClientService { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    public Type Render { get; } = typeof(SelectCluster);
    private string CurrentClusterName { get; set; } = default!;
    private bool _refresh;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (PveClientService.GetClustersNames().Any())
            {
                var clusterName = await PveClientService.GetCurrentClusterName();
                if (string.IsNullOrEmpty(clusterName))
                {
                    clusterName = PveClientService.GetClustersNames().ToArray()[0].Key;
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
            if (PveClientService.GetClustersNames().Count == 1)
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
            return !string.IsNullOrEmpty(clusterName)
                    && await PveClientService.GetClient(clusterName) != null;
        }
        catch // (Exception ex)
        {
            return false;
        }
    }

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