﻿/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.UI;
using Corsinvest.ProxmoxVE.Admin.Core.Options;
using Corsinvest.ProxmoxVE.Admin.Core.Support.Subscription;
using Corsinvest.ProxmoxVE.Admin.Core.UI.Dialogs;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.Layout;

public partial class SelectCluster : AHComponentBase, IUIAppBarItem
{
    [Inject] private IPveClientService PveClientService { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IOptionsSnapshot<AdminOptions> AdminOptions { get; set; } = default!;

    public Type Render { get; } = typeof(SelectCluster);
    private string CurrentClusterName { get; set; } = default!;
    private bool _refresh;
    private Dictionary<ClusterNodeOptions, Info> Checks { get; } = [];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (PveClientService.GetClusters().Any())
            {
                var clusterName = await PveClientService.GetCurrentClusterNameAsync();
                if (string.IsNullOrEmpty(clusterName))
                {
                    clusterName = PveClientService.GetClusters().ToArray()[0].Name;
                    await PveClientService.SetCurrentClusterNameAsync(clusterName);
                }

                _refresh = false;
                await ValueChanged(clusterName);
                _refresh = true;
            }
            else
            {
                ShowDialogFastConfig();
            }

            //checks subscriptions
            Checks.AddRange(await Helper.CheckAsync(AdminOptions.Value));
            StateHasChanged();
        }
    }

    private async Task ValueChanged(string value)
    {
        if (await PveClientService.ClusterIsValidAsync(value))
        {
            await PveClientService.SetCurrentClusterNameAsync(value);
            CurrentClusterName = value;
            if (_refresh) { NavigationManager.Refresh(true); }
        }
        else
        {
            await PveClientService.SetCurrentClusterNameAsync(string.Empty);
            if (PveClientService.GetClusters().Count() == 1) { ShowDialogFastConfig(); }
        }
        StateHasChanged();
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