/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.System.Components.ClusterConfig;

public partial class NodesSettings(IAdminService adminService,
                                   DialogService dialogService,
                                   NotificationService notificationService) : IModelParameter<ClusterSettings>
{
    [Parameter, EditorRequired] public ClusterSettings Model { get; set; } = default!;
    [Parameter] public bool Disabled { get; set; }

    // Validator names must be unique in the form: one per row
    private string FieldName(ClusterNodeSettings node, string property) => $"Node{Model.Nodes.IndexOf(node)}.{property}";

    public Task DiscoverAsync()
        => PveAdminUIHelper.PopulateClusterSettingsAsync(adminService, Model, dialogService, notificationService, L);
}
