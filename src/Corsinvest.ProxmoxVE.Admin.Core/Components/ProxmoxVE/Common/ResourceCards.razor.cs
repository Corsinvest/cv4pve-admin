/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Common;

public partial class ResourceCards
{
    [Parameter] public IList<ClusterResourceItem> Items { get; set; } = [];
    [Parameter] public Dictionary<string, string> TagStyleColorMaps { get; set; } = [];
    [Parameter] public string? Style { get; set; }
    [Parameter] public RenderFragment? TemplateViewTypeSelector { get; set; }
    [Parameter] public RenderFragment? TemplateToolbar { get; set; }
    [Parameter] public EventCallback OnRefresh { get; set; }
    [Parameter] public IEnumerable<FilterDescriptor> Filters { get; set; } = [];
    [Parameter] public LogicalFilterOperator LogicalFilterOperator { get; set; } = LogicalFilterOperator.And;
    [Parameter] public FilterCaseSensitivity FilterCaseSensitivity { get; set; } = FilterCaseSensitivity.Default;
    [Parameter] public bool ShowSearchBox { get; set; } = true;
    [Parameter] public bool ShowOsInfo { get; set; }
    [Parameter] public RenderFragment<ClusterResourceItem>? Template { get; set; }

    private ClusterResourceType? CardResourceTypeFilter { get; set; }
    private string? CardStatusFilter { get; set; }
    private string? CardVmTypeFilter { get; set; }
    private IList<string> CardTagsFilter { get; set; } = [];
    private IList<string> CardNodesFilter { get; set; } = [];
    private SearchTextBox<ClusterResourceItem>? SearchTextBox { get; set; }

    private List<ClusterResourceItem> DataFilteredItems = [];
    private IEnumerable<FilterDescriptor> _searchFilters = [];

    public async Task ClearSearchAsync()
    {
        if (SearchTextBox != null) { await SearchTextBox.ClearAsync(); }
    }

    protected override void OnParametersSet() => DataFilteredItems = ComputeDataFilteredItems();

    private async Task OnSearchFiltersChanged(IEnumerable<FilterDescriptor> filters)
    {
        _searchFilters = filters;
        DataFilteredItems = ComputeDataFilteredItems();
        await InvokeAsync(StateHasChanged);
    }

    private List<ClusterResourceItem> ComputeDataFilteredItems()
    {
        var filters = Filters.Concat(_searchFilters).ToList();
        if (filters.Count == 0) { return [.. Items]; }
        return [.. Items.AsQueryable().Where(filters, LogicalFilterOperator, FilterCaseSensitivity)];
    }

    private List<ClusterResourceType> ResourceTypes => [.. DataFilteredItems.Select(a => a.ResourceType).Distinct().Order()];
    private List<string> StatusTypes => [.. DataFilteredItems.Select(a => a.Status).Distinct().Order()];

    private List<string> VmTypes
        => [.. DataFilteredItems.Where(a => a.ResourceType == ClusterResourceType.Vm
                                        && (CardResourceTypeFilter == null || CardResourceTypeFilter == ClusterResourceType.Vm))
                                .Select(a => a.Type)
                                .Distinct()
                                .Order()];

    private List<string> AvailableNodes
        => [.. DataFilteredItems.Where(a => !string.IsNullOrEmpty(a.Node))
                                .Select(a => a.Node)
                                .Distinct()
                                .Order()];

    private List<string> AvailableTags
        => [.. DataFilteredItems.Where(a => !string.IsNullOrEmpty(a.Tags))
                                .SelectMany(a => a.GetTagsArray())
                                .Distinct()
                                .Order()];
}
