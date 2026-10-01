/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.AutoSnap.Api;

namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Components;

public partial class JobDialog(IModuleService moduleService, ISettingsService settingsService) : IModelParameter<JobSchedule>
{
    [Parameter] public JobSchedule Model { get; set; } = default!;

    private Type? WebHookTabComponentType { get; set; }

    // The snapshot name is prefix + label + timestamp: its length depends on the timestamp
    // format of the cluster, so it cannot be a data annotation on the label.
    private string? LabelError
        => string.IsNullOrWhiteSpace(Model.Label)
            ? null
            : AutoSnapEngine.ValidateLabelAndTimestampFormat(Model.Label,
                                                             settingsService.GetForModule<Module, Settings>(Model.ClusterName).TimestampFormat);

    protected override void OnInitialized()
        => WebHookTabComponentType = moduleService.Get<Module>()!.WebHookTabComponentType;
}
