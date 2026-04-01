/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Common;

public partial class ResourcesPickerDialog(DialogService DialogService) : IClusterNames
{
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }

    [Parameter] public IEnumerable<string> ClusterNames { get; set; } = [];
    [Parameter] public EventCallback<IEnumerable<string>> ClusterNamesChanged { get; set; } = default!;
    [Parameter] public string? Style { get; set; }
    [Parameter] public bool UseProgressBarPercentage { get; set; } = true;
    [Parameter] public bool DescriptionAsLink { get; set; } = true;
    [Parameter] public DataGridSettings DataGridSettings { get; set; } = new();
    [Parameter] public EventCallback<DataGridSettings> DataGridSettingsChanged { get; set; } = default!;
    [Parameter] public Func<ClusterResourceItem, string, bool>? FilterExpression { get; set; }
    [Parameter] public Func<ClusterResourceItem, string, bool>? SelectedItemExpression { get; set; }
    [Parameter] public bool ShowSnapshotSize { get; set; }
    [Parameter] public bool ShowOsInfo { get; set; }
    [Parameter] public RenderFragment<ClusterResourceItem> Template { get; set; } = default!;
    [Parameter] public bool ShowLoading { get; set; }
    [Parameter] public bool AvailableCommands { get; set; }
    [Parameter] public ResourcesViewPropertyIconStatus PropertyIconStatus { get; set; } = ResourcesViewPropertyIconStatus.None;
    [Parameter] public ResourceColumnIconStatus IconStatus { get; set; } = ResourceColumnIconStatus.IconAndText;
    [Parameter] public ClusterResourceType ResourceType { get; set; } = ClusterResourceType.All;
    [Parameter] public RenderFragment? TemplateToolbar { get; set; }
    [Parameter] public DataGridSelectionMode SelectionMode { get; set; } = DataGridSelectionMode.Single;
    [Parameter] public IList<ClusterResourceItem> SelectedItems { get; set; } = [];
    [Parameter] public EventCallback<IList<ClusterResourceItem>> SelectedItemsChanged { get; set; }
    [Parameter] public HashSet<string> PickableColumns { get; set; } = [];
    [Parameter] public bool AllowColumnPicking { get; set; } = true;
    [Parameter] public int RefreshInterval { get; set; }
    [Parameter] public bool ShowSearchBox { get; set; } = true;

    private void OnSelect() => DialogService.Close(SelectedItems.ToArray());
}
