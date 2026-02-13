/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Models.Parameters;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

namespace Corsinvest.ProxmoxVE.Admin.Core.Search.Providers;

public class ModuleSearchProvider(IModuleService moduleService,
                                  ISettingsService settingsService,
                                  IPermissionService permissionService) : ISearchProvider
{
    public string Id => "ModuleSearch";
    public string DisplayName => "Modules";
    public bool RequireClusterName => false;

    public IEnumerable<SearchFilter> Filters => [];
    public IEnumerable<SearchCommand> Commands => [];

    public async Task<IEnumerable<SearchResultItem>> SearchAsync(SearchContext context)
        => context.IsCommandSearch
            ? []
            : (await GetAvailableLinksAsync(context.ClusterName ?? string.Empty)).Select(link => new SearchResultItem
            {
                Title = link.Text,
                Subtitle = link.Module.Description,
                Icon = link.Icon,
                Color = link.IconColor,
                ResultType = SearchResultType.Module,
                Category = nameof(SearchResultType.Module),
                Url = link.RealUrl
            }).Where(a => a.MatchesSearch(context.SearchText));

    private async Task<IEnumerable<ModuleLinkBase>> GetAvailableLinksAsync(string clusterName)
    {
        var links = moduleService.Modules
                                 .Where(a => a.ModuleType == ModuleType.Application && a.Link?.Enabled == true)
                                 .Select(a => a.Link!)
                                 .OrderBy(a => a.OrderIndex)
                                 .ThenBy(a => a.Text)
                                 .ToList();

        var existsSettings = settingsService.GetEnabledClustersSettings().Any();
        var selectedCluster = string.IsNullOrEmpty(clusterName);

        foreach (var item in links.ToArray())
        {
            if (!await item.HasPermissionAsync(permissionService, clusterName)
                || (selectedCluster && item.Module.Scope == ClusterScope.Single)
                || !existsSettings)
            {
                links.Remove(item);
            }
        }

        return links;
    }

    public Task<DataSourceResult> GetDataSourceDialogAsync(string dataSource, DataSourceContext context)
        => Task.FromResult(DataSourceResult.Empty);
}
