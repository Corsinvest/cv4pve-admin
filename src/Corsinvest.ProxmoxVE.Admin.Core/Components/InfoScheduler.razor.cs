/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

public partial class InfoScheduler<TModule>(IDialogServiceEx dialogServiceEx,
                                            IModuleService moduleService) : IClusterName where TModule : ModuleBase
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;
    [Parameter] public IJobSchedule JobSchedule { get; set; } = default!;

    private bool CanEditSettings { get; set; }

    protected override async Task OnInitializedAsync()
        => CanEditSettings = await moduleService.Get<TModule>()!.HasPermissionEditorSettingsAsync(PermissionService, ClusterName);

    private Task OpenSettingsAsync() => dialogServiceEx.OpenSettingsAsync<TModule>(ClusterName);
}
