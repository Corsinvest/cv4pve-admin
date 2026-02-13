/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Cluster;

public partial class SelectResourceExDialog(DialogService DialogService)
{
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }

    [Parameter] public IEnumerable<string> ClusterNames { get; set; } = [];
    [Parameter] public EventCallback<IEnumerable<string>> ClusterNamesChanged { get; set; } = default!;
    [Parameter] public string? Style { get; set; }
    [Parameter] public string DialogStyle { get; set; } = "height: calc(100vh - 90px);";
    [Parameter] public bool UseProgressBarPercentage { get; set; } = true;
    [Parameter] public bool DescriptionAsLink { get; set; } = true;
    [Parameter] public DataGridSettings DataGridSettings { get; set; } = new();
    [Parameter] public EventCallback<DataGridSettings> DataGridSettingsChanged { get; set; } = default!;
    [Parameter] public Func<ClusterResourceEx, string, bool>? Filter { get; set; }
    [Parameter] public bool ShowSnapshotSize { get; set; }
    [Parameter] public bool ShowOsInfo { get; set; }
    [Parameter] public RenderFragment<ClusterResourceEx> Template { get; set; } = default!;
    [Parameter] public bool ShowLoading { get; set; }
    [Parameter] public bool AvailableCommands { get; set; }
    [Parameter] public ResourcesExPropertyIconStatus PropertyIconStatus { get; set; } = ResourcesExPropertyIconStatus.None;
    [Parameter] public ResourceColumnIconStatus IconStatus { get; set; } = ResourceColumnIconStatus.IconAndText;
    [Parameter] public ClusterResourceType ResourceType { get; set; } = ClusterResourceType.All;
    [Parameter] public RenderFragment? TemplateToolbar { get; set; }
    [Parameter] public DataGridSelectionMode SelectionMode { get; set; } = DataGridSelectionMode.Single;
    [Parameter] public IList<ClusterResourceEx> SelectedItems { get; set; } = [];
    [Parameter] public EventCallback<IList<ClusterResourceEx>> SelectedItemsChanged { get; set; }
    [Parameter] public HashSet<string> PickableColumns { get; set; } = [];
    [Parameter] public bool AllowColumnPicking { get; set; } = true;
    [Parameter] public int RefreshInterval { get; set; }

    private void OnSelect() => DialogService.Close(SelectedItems.ToArray());
}
