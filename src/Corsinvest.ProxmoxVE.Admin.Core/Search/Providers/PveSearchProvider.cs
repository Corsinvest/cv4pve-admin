/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Vm;
using Corsinvest.ProxmoxVE.Admin.Core.Models.Parameters;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Mapster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Search.Providers;

public class PveSearchProvider : ISearchProvider
{
    public string Id => "PveSearch";
    public string DisplayName => "Proxmox VE";
    public bool RequireClusterName => true;

    private readonly IAdminService _adminService;
    private readonly IPermissionService _permissionService;

    private readonly ParameterMetadata _vmParamVmStopped;
    private readonly ParameterMetadata _vmParamVmRunning;
    private readonly ParameterMetadata _nameParam;
    private readonly ParameterMetadata _descriptionParam;
    private readonly ParameterMetadata _vmStateParam;

    public PveSearchProvider(IAdminService adminService, IPermissionService permissionService)
    {
        _adminService = adminService;
        _permissionService = permissionService;

        _vmParamVmStopped = new ParameterMetadata("vm",
                                                  "VM/CT",
                                                  "Select VM or Container",
                                                  ParameterType.Select,
                                                  true,
                                                  null,
                                                  new ParameterOptions(DataSource: ctx => GetDataSourceAsync(DataSourceType.VmStopped, ctx), Placeholder: "e.g. 100, debian12"));

        _vmParamVmRunning = new ParameterMetadata("vm",
                                                  "VM/CT",
                                                  "Select VM or Container",
                                                  ParameterType.Select,
                                                  true,
                                                  null,
                                                  new ParameterOptions(DataSource: ctx => GetDataSourceAsync(DataSourceType.VmRunning, ctx), Placeholder: "e.g. 100, debian12"));

        _nameParam = new ParameterMetadata("name", "Name", "Snapshot name", ParameterType.Text, true, null, new ParameterOptions(Placeholder: "e.g. snap-backup-01"));
        _descriptionParam = new ParameterMetadata("description", "Description", "Optional description", ParameterType.Text, false, null, null);
        _vmStateParam = new ParameterMetadata("vmstate", "Include RAM", "Save memory state", ParameterType.Bool, false, false, null);
    }

    // Filter definitions
    public static readonly SearchFilter VmFilter = new("vm", "Vm/Ct", PveAdminUIHelper.Icons.GetVmType(VmType.Qemu), BadgeStyle.Success);
    public static readonly SearchFilter IpFilter = new("ip", "Ip address", "lan", BadgeStyle.Success);
    public static readonly SearchFilter NodeFilter = new("node", "Node", PveAdminUIHelper.Icons.Node, BadgeStyle.Info);
    //public static readonly SearchFilter StorageFilter = new("storage", "Storage", PveAdminUIHelper.Icons.Storage, BadgeStyle.Dark);
    //public static readonly SearchFilter PoolFilter = new("pool", "Pool", PveAdminUIHelper.Icons.Pool, BadgeStyle.Light);

    public IEnumerable<SearchFilter> Filters => [VmFilter, NodeFilter, IpFilter /*,StorageFilter, PoolFilter*/];

    public IEnumerable<SearchCommand> Commands =>
    [
        new("start", "Start", "Start VM/Container", "play_arrow", [_vmParamVmStopped], StartAsync),
        new("stop", "Stop", "Stop VM/Container", "stop", [_vmParamVmRunning], StopAsync),
        new("restart", "Restart", "Restart VM/Container", "restart_alt", [_vmParamVmRunning], RestartAsync),
        new("console", "Console", "Open console", "terminal", [_vmParamVmRunning], OpenConsoleAsync),
        new("create snapshot", "Create Snapshot", "Create VM/CT snapshot", "add_a_photo", [_vmParamVmRunning, _nameParam, _descriptionParam, _vmStateParam], SnapshotAsync),
    ];

    #region Command Implementations
    private async Task ChangeStatusAsync(CommandExecutionContext context, VmStatus status)
        => await context.ServiceProvider.GetCommandExecutor()
                                        .ExecuteAsync(new VmChangeStateCommand(context.ClusterName,
                                                                                      _vmParamVmRunning.ToLong(context.Parameters),
                                                                                      status));

    private Task StartAsync(CommandExecutionContext context) => ChangeStatusAsync(context, VmStatus.Start);
    private Task StopAsync(CommandExecutionContext context) => ChangeStatusAsync(context, VmStatus.Stop);
    private Task RestartAsync(CommandExecutionContext context) => ChangeStatusAsync(context, VmStatus.Reboot);

    private async Task OpenConsoleAsync(CommandExecutionContext context)
    {
        var clusterClient = _adminService[context.ClusterName];
        switch (clusterClient.Settings.WebApi.AccessType)
        {
            case ClusterAccessType.Credential:
                var client = await clusterClient.GetPveClientAsync();
                var vm = await client.GetVmAsync(_vmParamVmRunning.ToLong(context.Parameters));

                var browserService = context.ServiceProvider.GetRequiredService<IBrowserService>();
                await browserService.OpenPveConsole(clusterClient.GetUrlWebConsole(vm.Node, vm.VmType, vm.VmId, vm.Name, false));
                break;

            case ClusterAccessType.ApiToken:
                var notificationService = context.ServiceProvider.GetRequiredService<NotificationService>();
                var L = context.ServiceProvider.GetRequiredService<IStringLocalizer<PveSearchProvider>>();
                notificationService.Info(L["Console not supported with API Token access. Please use Credential access type to enable this feature."]);
                break;
            default:
                break;
        }
    }

    private async Task SnapshotAsync(CommandExecutionContext context)
        => await context.ServiceProvider.GetCommandExecutor()
                                        .ExecuteAsync(new VmCreateSnapshotCommand(context.ClusterName,
                                                                                         _vmParamVmRunning.ToLong(context.Parameters),
                                                                                         _nameParam.ToString(context.Parameters),
                                                                                         _descriptionParam.ToString(context.Parameters),
                                                                                         _vmStateParam.ToBoolean(context.Parameters)));
    #endregion

    public async Task<IEnumerable<SearchResultItem>> SearchAsync(SearchContext context)
    {
        if (ApplicationHelper.IsAllCluster(context.ClusterName)) { return []; }

        var result = new List<SearchResultItem>();
        if (context.IsCommandSearch)
        {
            result = [.. SearchResultItem.CommandsToResult(this)];
        }
        else
        {
            var clusterClient = _adminService[context.ClusterName];

            async Task<IEnumerable<ClusterResource>> GetDataAsync(ClusterResourceType type)
            {
                var item = (await clusterClient.CachedData.GetResourcesAsync(false))
                                .Where(a => a.ResourceType == type);

                return await _permissionService.FilterAsync(context.ClusterName, item);
            }

            if (context.TryGetFilter(NodeFilter, out _))
            {
                result.AddRange((await GetDataAsync(ClusterResourceType.Node)).Select(a => new SearchResultItem
                {
                    Title = a.Id,
                    Subtitle = a.Description,
                    Icon = PveAdminUIHelper.Icons.Node,
                    Color = Colors.Info,
                    ResultType = SearchResultType.Item,
                    Category = "Node",
                    Url = UrlHelper.Resources.NodeUrl(a.Node, context.ClusterName),
                    Tags = [new(a.Status, PveAdminUIHelper.ToBadgeStyle(PveAdminUIHelper.GetResourcesColorStatus(a.Status, false)))],
                    Data = a
                }));
            }
            //else if (context.TryGetFilter(StorageFilter, out _))
            //{
            //    result.AddRange((await GetDataAsync(ClusterResourceType.Storage)).Select(a => new SearchResultItem
            //    {
            //        Title = a.Storage,
            //        Subtitle = $"{a.Node} - {a.PluginType}",
            //        Icon = PveAdminUIHelper.Icons.Storage,
            //        Color = Colors.InfoDark,
            //        ResultType = SearchResultType.Item,
            //        Category = "Storage",
            //        //Url = $"/pve/node/{a.Node}/storage/{a.Storage}",
            //        Tags = [new(a.Status, PveAdminUIHelper.ToBadgeStyle(PveAdminUIHelper.GetResourcesColorStatus(a.Status, false)))],
            //        Data = a
            //    }));
            //}
            else if (context.TryGetFilter(VmFilter, out _) || context.TryGetFilter(IpFilter, out _))
            {
                var vms = (await clusterClient.CachedData.GetResourcesAsync(false))
                                .Where(a => a.ResourceType == ClusterResourceType.Vm && a.VmType == VmType.Qemu);

                vms = await _permissionService.FilterAsync(context.ClusterName, vms);

                var data = vms.AsQueryable()
                                .ProjectToType<ClusterResourceEx>()
                                .ToList()
                                .ConvertAll(item =>
                                {
                                    item.ClusterName = context.ClusterName;
                                    return item;
                                });

                if (context.TryGetFilter(IpFilter, out _))
                {
                    foreach (var item in data)
                    {
                        item.Set(await clusterClient.CachedData.GetVmOsInfoAsync(item, false));
                    }
                }

                result.AddRange(data.Select(a =>
                {
                    var extraInfo = (a.Tags + "")
                                     .Split(";", StringSplitOptions.RemoveEmptyEntries)
                                     .ToList();

                    var tags = new List<SearchTag>
                    {
                        new(a.Status, PveAdminUIHelper.ToBadgeStyle(PveAdminUIHelper.GetResourcesColorStatus(a.Status, a.IsLocked)))
                    };

                    if (a.IsLocked) { tags.Add(new(a.Lock, PveAdminUIHelper.ToBadgeStyle(Colors.Warning))); }

                    if (!string.IsNullOrEmpty(a.HostName) && !a.HostName.StartsWith("Error"))
                    {
                        tags.Add(new(a.HostName, PveAdminUIHelper.ToBadgeStyle(Colors.Info)));
                    }

                    if (!string.IsNullOrEmpty(a.OsVersion))
                    {
                        tags.Add(new(a.OsVersion, PveAdminUIHelper.ToBadgeStyle(Colors.Info)));
                    }

                    return new SearchResultItem
                    {
                        Title = a.Id,
                        Subtitle = a.Description,
                        Icon = PveAdminUIHelper.Icons.GetVmType(a.VmType),
                        Color = Colors.Success,
                        ResultType = SearchResultType.Item,
                        Category = a.VmType == VmType.Qemu
                                    ? "Virtual Machine"
                                    : "Container",
                        Url = UrlHelper.Resources.VmUrl(a.VmId, context.ClusterName),
                        Tags = tags,
                        ExtraInfo = extraInfo,
                        Data = a
                    };
                }));
            }
            //else if (context.TryGetFilter(PoolFilter, out _))
            //{
            //    result.AddRange((await GetDataAsync(ClusterResourceType.Pool)).Select(a => new SearchResultItem
            //    {
            //        Title = a.Pool,
            //        Subtitle = a.Description,
            //        Icon = PveAdminUIHelper.Icons.Pool,
            //        Color = Colors.InfoLight,
            //        ResultType = SearchResultType.Item,
            //        Category = "Pool",
            //        //Url = $"/pve/pool/{a.Pool}"
            //        Data = a
            //    }));
            //}
        }

        return result.Where(a => a.MatchesSearch(context.SearchText));
    }

    private enum DataSourceType
    {
        VmStopped,
        VmRunning,
        Vms,
        Nodes,
        Storages,
        Pools
    }

    private async Task<DataSourceResult> GetDataSourceAsync(DataSourceType type, DataSourceContext context)
    {
        if (ApplicationHelper.IsAllCluster(context.ClusterName)) { return DataSourceResult.Empty; }

        var clusterClient = _adminService[context.ClusterName];
        var resources = await clusterClient.CachedData.GetResourcesAsync(false);

        DataSourceResult GetVms(Func<ClusterResource, bool> where)
        {
            return new DataSourceResult(
                   Data: resources.Where(a => a.ResourceType == ClusterResourceType.Vm)
                                  .Where(where)
                                  .Select(a => new { a.VmId, a.Description, a.Status })
                                  .ToList(),
                   TextProperty: nameof(ClusterResource.Description),
                   ValueProperty: nameof(ClusterResource.VmId),
                   Columns:
                   [
                        new(nameof(ClusterResource.VmId), "VmId", "80px"),
                        new(nameof(ClusterResource.Description), "Description"),
                        new(nameof(ClusterResource.Status), "Status", "80px")
                   ]);
        }

        return type switch
        {
            DataSourceType.VmStopped => GetVms(a => !a.IsRunning && !a.IsUnknown),
            DataSourceType.VmRunning => GetVms(a => a.IsRunning),
            DataSourceType.Vms => GetVms(_ => true),

            DataSourceType.Nodes => new DataSourceResult(
                Data: resources.Where(r => r.ResourceType == ClusterResourceType.Node)
                               .Select(a => new { a.Node, a.Status })
                               .ToList(),
                TextProperty: nameof(ClusterResource.Node),
                ValueProperty: nameof(ClusterResource.Node),
                Columns:
                [
                    new(nameof(ClusterResource.Node), "Node", "120px"),
                    new(nameof(ClusterResource.Status), "Status", "80px")
                ]
            ),

            DataSourceType.Storages => new DataSourceResult(
                Data: resources.Where(r => r.ResourceType == ClusterResourceType.Storage)
                                .Select(a => new { a.Storage, a.Node, a.PluginType, a.Status })
                                .ToList(),
                TextProperty: nameof(ClusterResource.Storage),
                ValueProperty: nameof(ClusterResource.Storage),
                Columns:
                [
                    new(nameof(ClusterResource.Storage), "Storage", "120px"),
                    new(nameof(ClusterResource.Description), "Node", "100px"),
                    new(nameof(ClusterResource.Node), "Node", "100px"),
                    new(nameof(ClusterResource.PluginType), "Type", "80px"),
                    new(nameof(ClusterResource.Status), "Status", "80px")
                ]
            ),

            DataSourceType.Pools => new DataSourceResult(
                Data: resources.Where(r => r.ResourceType == ClusterResourceType.Pool)
                               .Select(a => new
                               {
                                   a.Pool,
                                   a.Description
                               })
                               .ToList(),
                TextProperty: nameof(ClusterResource.Pool),
                ValueProperty: nameof(ClusterResource.Pool),
                Columns:
                [
                    new(nameof(ClusterResource.Pool), "Storage", "120px"),
                    new(nameof(ClusterResource.Description), "Node", "100px"),
                    new(nameof(ClusterResource.Status), "Status", "80px")
                ]
            ),

            _ => DataSourceResult.Empty
        };
    }
}
