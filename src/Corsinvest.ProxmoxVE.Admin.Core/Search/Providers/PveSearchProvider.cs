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

    private readonly ParameterMetadata VmParamVmStopped;
    private readonly ParameterMetadata VmParamVmRunning;
    private readonly ParameterMetadata NameParam;
    private readonly ParameterMetadata DescriptionParam;
    private readonly ParameterMetadata VmStateParam;

    public PveSearchProvider(IAdminService adminService, IPermissionService permissionService)
    {
        _adminService = adminService;
        _permissionService = permissionService;

        VmParamVmStopped = new ParameterMetadata(
            "vm",
            "VM/CT",
            "Select VM or Container",
            ParameterType.Select,
            true,
            null,
            new ParameterOptions(DataSource: ctx => GetDataSourceAsync(DataSourceType.VmStopped, ctx), Placeholder: "e.g. 100, debian12"));

        VmParamVmRunning = new ParameterMetadata(
            "vm",
            "VM/CT",
            "Select VM or Container",
            ParameterType.Select,
            true,
            null,
            new ParameterOptions(DataSource: ctx => GetDataSourceAsync(DataSourceType.VmRunning, ctx), Placeholder: "e.g. 100, debian12"));

        NameParam = new ParameterMetadata("name", "Name", "Snapshot name", ParameterType.Text, true, null, new ParameterOptions(Placeholder: "e.g. snap-backup-01"));
        DescriptionParam = new ParameterMetadata("description", "Description", "Optional description", ParameterType.Text, false, null, null);
        VmStateParam = new ParameterMetadata("vmstate", "Include RAM", "Save memory state", ParameterType.Bool, false, false, null);
    }

    // Filter definitions
    public static readonly SearchFilter VmFilter = new("vm", "Vm/Ct", PveAdminUIHelper.Icons.GetVmType(VmType.Qemu), BadgeStyle.Success);
    public static readonly SearchFilter IpFilter = new("ip", "Ip address", "lan", BadgeStyle.Success);
    public static readonly SearchFilter NodeFilter = new("node", "Node", PveAdminUIHelper.Icons.Node, BadgeStyle.Info);
    public static readonly SearchFilter StorageFilter = new("storage", "Storage", PveAdminUIHelper.Icons.Storage, BadgeStyle.Dark);
    public static readonly SearchFilter PoolFilter = new("pool", "Pool", PveAdminUIHelper.Icons.Pool, BadgeStyle.Light);

    public IEnumerable<SearchFilter> Filters => [VmFilter, NodeFilter, StorageFilter, PoolFilter, IpFilter];

    public IEnumerable<SearchCommand> Commands =>
    [
        new("start", "Start", "Start VM/Container", "play_arrow", [VmParamVmStopped], StartAsync),
        new("stop", "Stop", "Stop VM/Container", "stop", [VmParamVmRunning], StopAsync),
        new("restart", "Restart", "Restart VM/Container", "restart_alt", [VmParamVmRunning], RestartAsync),
        new("console", "Console", "Open console", "terminal", [VmParamVmRunning], OpenConsoleAsync),
        new("create snapshot", "Create Snapshot", "Create VM/CT snapshot", "add_a_photo", [VmParamVmRunning, NameParam, DescriptionParam, VmStateParam], SnapshotAsync),
    ];

    #region Command Implementations
    private async Task ChangeStatusAsync(CommandExecutionContext context, VmStatus status)
        => await context.ServiceProvider.GetCommandExecutor()
                                        .ExecuteAsync(new VmChangeStateCommand(context.ClusterName,
                                                                                      VmParamVmRunning.ToLong(context.Parameters),
                                                                                      status));

    private Task StartAsync(CommandExecutionContext context) => ChangeStatusAsync(context, VmStatus.Start);
    private Task StopAsync(CommandExecutionContext context) => ChangeStatusAsync(context, VmStatus.Stop);
    private Task RestartAsync(CommandExecutionContext context) => ChangeStatusAsync(context, VmStatus.Reboot);

    private async Task OpenConsoleAsync(CommandExecutionContext context)
    {
        var client = await _adminService[context.ClusterName].GetPveClientAsync();
        var vm = await client.GetVmAsync(VmParamVmRunning.ToLong(context.Parameters));

        var browserService = context.ServiceProvider.GetRequiredService<IBrowserService>();
        await browserService.OpenPveConsole(_adminService[context.ClusterName].GetUrlWebConsole(vm.Node, vm.VmType, vm.VmId, vm.Name, false));
    }

    private async Task SnapshotAsync(CommandExecutionContext context)
        => await context.ServiceProvider.GetCommandExecutor()
                                        .ExecuteAsync(new VmCreateSnapshotCommand(context.ClusterName,
                                                                                         VmParamVmRunning.ToLong(context.Parameters),
                                                                                         NameParam.ToString(context.Parameters),
                                                                                         DescriptionParam.ToString(context.Parameters),
                                                                                         VmStateParam.ToBoolean(context.Parameters)));
    #endregion

    public async Task<IEnumerable<SearchResultItem>> SearchAsync(SearchContext context)
    {
        if (string.IsNullOrEmpty(context.ClusterName)) { return []; }

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
                    //Url = $"/pve/node/{a.Node}",
                    Tags = [new(a.Status, PveAdminUIHelper.ToBadgeStyle(PveAdminUIHelper.GetResourcesColorStatus(a.Status, false)))]
                }));
            }
            else if (context.TryGetFilter(StorageFilter, out _))
            {
                result.AddRange((await GetDataAsync(ClusterResourceType.Storage)).Select(a => new SearchResultItem
                {
                    Title = a.Storage,
                    Subtitle = $"{a.Node} - {a.PluginType}",
                    Icon = PveAdminUIHelper.Icons.Storage,
                    Color = Colors.InfoDark,
                    ResultType = SearchResultType.Item,
                    Category = "Storage",
                    //Url = $"/pve/node/{a.Node}/storage/{a.Storage}",
                    Tags = [new(a.Status, PveAdminUIHelper.ToBadgeStyle(PveAdminUIHelper.GetResourcesColorStatus(a.Status, false)))]
                }));
            }
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
                        Category = a.VmType == VmType.Qemu ? "Virtual Machine" : "Container",
                        //Url = $"/pve/vms/{a.VmId}",
                        Tags = tags,
                        ExtraInfo = extraInfo
                    };
                }));
            }
            else if (context.TryGetFilter(PoolFilter, out _))
            {
                result.AddRange((await GetDataAsync(ClusterResourceType.Pool)).Select(a => new SearchResultItem
                {
                    Title = a.Pool,
                    Subtitle = a.Description,
                    Icon = PveAdminUIHelper.Icons.Pool,
                    Color = Colors.InfoLight,
                    ResultType = SearchResultType.Item,
                    Category = "Pool",
                    //Url = $"/pve/pool/{a.Pool}"
                }));
            }

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
        var clusterName = context.ClusterName;
        if (string.IsNullOrEmpty(clusterName)) { return DataSourceResult.Empty; }

        var clusterClient = _adminService[clusterName];
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
            DataSourceType.Vms => GetVms(_ => true == true),

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
