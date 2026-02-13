/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.System.Components.ClusterConfig;

public partial class ClusterSettingsDialog : IModelParameter<ClusterSettings>
{
    [Parameter] public ClusterSettings Model { get; set; } = default!;

    private bool NodesDisabled
        => Model.AccessType switch
        {
            ClusterAccessType.Credential => string.IsNullOrWhiteSpace(Model.ApiCredential.Username) || string.IsNullOrWhiteSpace(Model.ApiCredential.Password),
            ClusterAccessType.ApiToken => string.IsNullOrWhiteSpace(Model.ApiToken),
            _ => false
        };
}
