/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;

namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Components;

public partial class JobDialog(IModuleService moduleService) : IModelParameter<JobSchedule>
{
    [Parameter] public JobSchedule Model { get; set; } = default!;

    private Type? WebHookTabComponentType { get; set; } = default!;

    protected override void OnInitialized()
        => WebHookTabComponentType = moduleService.Get<Module>()!.WebHookTabComponentType;
}
