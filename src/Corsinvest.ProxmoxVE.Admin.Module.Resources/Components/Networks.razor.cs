/*
using static Corsinvest.ProxmoxVE.Admin.Core.BuildInfo;
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */

using Corsinvest.ProxmoxVE.Admin.Core;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Extension.Utils;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components;

public partial class Networks(IAdminService adminService,
                              ISettingsService settingsService,
                              IBrowserService browserService) : IClusterName, IRefreshableData
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private List<VmNetwork> VmNetworks { get; set; } = [];
    private List<NodeNetwork> NodeNetworks { get; set; } = [];
    private List<SdnVnet> SdnVnets { get; set; } = [];
    private bool IsLoadingNodes { get; set; }
    private bool IsLoadingGuests { get; set; }
    private bool IsLoadingSdn { get; set; }
    private RadzenDataGrid<VmNetwork> DataGridRef { get; set; } = default!;
    private int SelectedTab { get; set; }
    private bool _nodesLoaded;
    private bool _guestsLoaded;
    private bool _sdnLoaded;

    private record NodeNetwork(string Node,
                               bool Active,
                               bool AutoStart,
                               bool? Exists,
                               string Type,
                               string Interface,
                               string LinkType,
                               string Method,
                               string Cidr,
                               string Address,
                               string Netmask,
                               string Gateway,
                               string Method6,
                               string Cidr6,
                               string Address6,
                               int? Netmask6,
                               string Gateway6,
                               int Priority,
                               int? Mtu,
                               string BondMode,
                               string BondMiimon,
                               string BondPrimary,
                               string BondXmitHashPolicy,
                               string Slaves,
                               string BridgeStp,
                               bool? BridgeVlanAware,
                               string BridgeVids,
                               string BridgeFd,
                               string BridgePorts,
                               int? VlanId,
                               string VlanRawDevice,
                               string VlanProtocol,
                               string OvsBridge,
                               string OvsBonds,
                               string OvsPorts,
                               string OvsOptions,
                               int? OvsTag,
                               int? VxlanId,
                               string VxlanLocalTunnelIp,
                               string VxlanPhysDev,
                               string Comments,
                               string Comments6);

    private record SdnVnet(string Vnet,
                           string Zone,
                           string ZoneType,
                           string? ZoneBridge,
                           int? Tag,
                           string? Alias,
                           string Nodes);

    private record VmNetwork(string Node,
                             long VmId,
                             string Name,
                             string Guest,
                             string Type,
                             string Status,
                             bool IsLocked,
                             string? Hostname,
                             bool IsInternal,
                             string NetId,
                             string? MacAddress,
                             string? Bridge,
                             int? Tag,
                             string? Model,
                             bool Firewall,
                             string IpAddress,
                             string IpAddress6,
                             string? Gateway,
                             string? Gateway6,
                             int? Mtu,
                             double? Rate);

    private bool _diagramLoaded;
    private bool IsLoadingDiagram { get; set; }
    private string NetworkDiagramSvg { get; set; } = string.Empty;

    private bool _groupAdded;

    protected override Task OnInitializedAsync() => RefreshDataAsync();

    protected override void OnAfterRender(bool firstRender)
    {
        if (!_groupAdded && DataGridRef is not null)
        {
            _groupAdded = true;
            DataGridRef.Groups.Add(new()
            {
                Title = L["Guest"],
                Property = nameof(VmNetwork.Guest)
            });
            StateHasChanged();
        }
    }

    public async Task RefreshDataAsync()
    {
        switch (SelectedTab)
        {
            case 0:
                _nodesLoaded = false;
                await LoadNoteNetworksAsync();
                break;
            case 1:
                _guestsLoaded = false;
                await LoadVmNetworksAsync();
                break;
            case 2:
                _sdnLoaded = false;
                await LoadSdnAsync();
                break;
            default:
                _diagramLoaded = false;
                await LoadDiagramAsync();
                break;
        }
    }

    public async Task OnTabChangeAsync(int index)
    {
        SelectedTab = index;
        switch (index)
        {
            case 0 when !_nodesLoaded: await LoadNoteNetworksAsync(); break;
            case 1 when !_guestsLoaded: await LoadVmNetworksAsync(); break;
            case 2 when !_sdnLoaded: await LoadSdnAsync(); break;
            case 3 when !_diagramLoaded: await LoadDiagramAsync(); break;
        }
    }

    public async Task LoadDiagramAsync()
    {
        IsLoadingDiagram = true;
        NetworkDiagramSvg = string.Empty;
        await InvokeAsync(StateHasChanged);

        var clusterClient = adminService[ClusterName];
        var client = await clusterClient.GetPveClientAsync();

        var resources = await clusterClient.CachedData.GetResourcesAsync(false);
        var nodes = resources.Where(a => a.ResourceType == ClusterResourceType.Node).ToList();
        var vms = resources.Where(a => a.ResourceType == ClusterResourceType.Vm).ToList();

        // Node networks
        var hostNets = (await ParallelHelper.RunManyAsync(nodes, async item =>
        {
            var nets = await client.Nodes[item.Node].Network.GetAsync();
            return nets.Select(n => new NetworkDiagramBuilder.NodeNetworkRow(item.Node, n));
        })).ToList();

        // VM networks
        var vmNets = (await ParallelHelper.RunManyAsync(vms, async item =>
        {
            var config = await clusterClient.CachedData.GetGuestConfigAsync(item.Node, item.VmType, item.VmId, false);
            var networks = config?.Networks ?? [];
            switch (item.VmType)
            {
                case VmType.Qemu:
                    var qemuNetsTask = clusterClient.CachedData.GetQemuNetworkAsync(item.Node, item.VmId, false).AsTask();
                    var hostnameTask = clusterClient.CachedData.GetQemuHostnameAsync(item.Node, item.VmId, false).AsTask();
                    await Task.WhenAll(qemuNetsTask, hostnameTask);

                    var netDict = networks.Where(n => !string.IsNullOrEmpty(n.MacAddress))
                                          .ToDictionary(n => n.MacAddress!, StringComparer.OrdinalIgnoreCase);

                    return [.. qemuNetsTask.Result.Result
                                           .Where(a => !string.IsNullOrEmpty(a.HardwareAddress) && a.HardwareAddress != "00:00:00:00:00:00")
                                           .Select(net =>
                                           {
                                                var hasConfig = netDict.TryGetValue(net.HardwareAddress!, out var configNet);
                                                return new NetworkDiagramBuilder.VmNetworkRow(item.VmId,
                                                                                              item.Name,
                                                                                              item.Node,
                                                                                              item.Type,
                                                                                              item.Status,
                                                                                              hostnameTask.Result,
                                                                                              configNet ?? new(),
                                                                                              !hasConfig);
                                           })];

                case VmType.Lxc:
                    return networks.Select(n => new NetworkDiagramBuilder.VmNetworkRow(item.VmId,
                                                                                       item.Name,
                                                                                       item.Node,
                                                                                       item.Type,
                                                                                       item.Status,
                                                                                       (config as VmConfigLxc)?.Hostname,
                                                                                       n,
                                                                                       false));

                default: return [];
            }
        })).ToList();

        var storageTask = client.Storage.GetAsync();
        var sdnRowsTask = FetchSdnRowsAsync(client, nodes);
        await Task.WhenAll(storageTask, sdnRowsTask);

        var storages = storageTask.Result.ToList();

        NetworkDiagramSvg = NetworkDiagramBuilder.BuildSvg(
            hostNets,
            sdnRowsTask.Result,
            vmNets,
            storages,
            new NetworkDiagramBuilder.DiagramInfo(settingsService.GetAppSettings().AppName, ApplicationHelper.GitHubRepoUrl, BuildInfo.Version));

        IsLoadingDiagram = false;
        _diagramLoaded = true;
        await InvokeAsync(StateHasChanged);
    }

    private static async Task<List<NetworkDiagramBuilder.SdnVnetRow>> FetchSdnRowsAsync(PveClient client,
                                                                                       List<ClusterResource> nodes)
    {
        var vnetsTask = client.Cluster.Sdn.Vnets.GetAsync();
        var zonesTask = client.Cluster.Sdn.Zones.GetAsync();
        await Task.WhenAll(vnetsTask, zonesTask);

        var zones = zonesTask.Result;
        return [.. vnetsTask.Result.Select(v =>
        {
            var zone = zones.FirstOrDefault(z => z.Zone == v.Zone);
            var zoneNodes = string.IsNullOrEmpty(zone?.Nodes)
                ? (IReadOnlyList<string>)[.. nodes.Select(n => n.Node)]
                : [.. zone.Nodes.Split(',').Select(s => s.Trim())];
            return new NetworkDiagramBuilder.SdnVnetRow(
                Vnet: v.Vnet ?? "",
                Zone: v.Zone ?? "",
                ZoneType: zone?.Type ?? "simple",
                ZoneBridge: zone?.Bridge,
                Tag: v.Tag,
                Alias: v.Alias,
                Nodes: zoneNodes);
        })];
    }

    public async Task LoadSdnAsync()
    {
        IsLoadingSdn = true;
        SdnVnets = [];
        await InvokeAsync(StateHasChanged);

        var clusterClient = adminService[ClusterName];
        var client = await clusterClient.GetPveClientAsync();
        var nodes = (await clusterClient.CachedData.GetResourcesAsync(false))
                        .Where(a => a.ResourceType == ClusterResourceType.Node)
                        .ToList();

        var rows = await FetchSdnRowsAsync(client, nodes);
        SdnVnets = [.. rows.Select(r => new SdnVnet(r.Vnet,
                                                    r.Zone,
                                                    r.ZoneType,
                                                    r.ZoneBridge,
                                                    r.Tag,
                                                    r.Alias,
                                                    string.Join(", ", r.Nodes)))];

        IsLoadingSdn = false;
        _sdnLoaded = true;
        await InvokeAsync(StateHasChanged);
    }

    private async Task DownloadDiagramAsync()
    {
        if (string.IsNullOrEmpty(NetworkDiagramSvg)) { return; }
        await browserService.DownloadFileAsync($"network-diagram-{ClusterName}.svg",
                                               NetworkDiagramSvg,
                                               "image/svg+xml");
    }

    public async Task LoadNoteNetworksAsync()
    {
        IsLoadingNodes = true;
        NodeNetworks = [];

        await InvokeAsync(StateHasChanged);

        var clusterClient = adminService[ClusterName];
        var client = await adminService[ClusterName].GetPveClientAsync();

        var nodes = (await clusterClient.CachedData.GetResourcesAsync(false))
                        .Where(a => a.ResourceType == ClusterResourceType.Node);

        NodeNetworks = [.. (await ParallelHelper.RunManyAsync(nodes, async item =>
                        {
                            var networks = await client.Nodes[item.Node].Network.GetAsync();
                            return networks.Select(a => new NodeNetwork(item.Node,
                                                                        a.Active,
                                                                        a.AutoStart,
                                                                        a.Exists,
                                                                        a.Type,
                                                                        a.Interface,
                                                                        a.LinkType,
                                                                        a.Method,
                                                                        a.Cidr,
                                                                        a.Address,
                                                                        a.Netmask,
                                                                        a.Gateway,
                                                                        a.Method6,
                                                                        a.Cidr6,
                                                                        a.Address6,
                                                                        a.Netmask6,
                                                                        a.Gateway6,
                                                                        a.Priority,
                                                                        a.Mtu,
                                                                        a.BondMode,
                                                                        a.BondMiimon,
                                                                        a.BondPrimary,
                                                                        a.BondXmitHashPolicy,
                                                                        a.Slaves,
                                                                        a.BridgeStp,
                                                                        a.BridgeVlanAware,
                                                                        a.BridgeVids,
                                                                        a.BridgeFd,
                                                                        a.BridgePorts,
                                                                        a.VlanId,
                                                                        a.VlanRawDevice,
                                                                        a.VlanProtocol,
                                                                        a.OvsBridge,
                                                                        a.OvsBonds,
                                                                        a.OvsPorts,
                                                                        a.OvsOptions,
                                                                        a.OvsTag,
                                                                        a.VxlanId,
                                                                        a.VxlanLocalTunnelIp,
                                                                        a.VxlanPhysDev,
                                                                        a.Comments,
                                                                        a.Comments6));
                        }))
                        .OrderBy(a => a.Node)];

        IsLoadingNodes = false;
        _nodesLoaded = true;
        await InvokeAsync(StateHasChanged);
    }

    public async Task LoadVmNetworksAsync()
    {
        IsLoadingGuests = true;
        VmNetworks = [];

        await InvokeAsync(StateHasChanged);

        var clusterClient = adminService[ClusterName];

        var vms = (await clusterClient.CachedData.GetResourcesAsync(false))
                        .Where(a => a.ResourceType == ClusterResourceType.Vm)
                        .OrderBy(a => a.Node)
                        .ThenBy(a => a.Type)
                        .ThenBy(a => a.VmId);

        VmNetworks = [.. (await ParallelHelper.RunManyAsync(vms, async item =>
                        {
                            var config = await clusterClient.CachedData.GetGuestConfigAsync(item.Node, item.VmType, item.VmId, false);
                            var networks = config?.Networks ?? [];

                            switch (item.VmType)
                            {
                                case VmType.Qemu:
                                    var vmNetwors = await clusterClient.CachedData.GetQemuNetworkAsync(item.Node, item.VmId, false);
                                    var hostname = await clusterClient.CachedData.GetQemuHostnameAsync(item.Node, item.VmId, false);
                                    var netDict = networks.Where(n => !string.IsNullOrEmpty(n.MacAddress))
                                                          .ToDictionary(n => n.MacAddress, StringComparer.OrdinalIgnoreCase);

                                    return (IEnumerable<VmNetwork>)[.. vmNetwors.Result
                                        .Where(a => !string.IsNullOrEmpty(a.HardwareAddress) && a.HardwareAddress != "00:00:00:00:00:00")
                                        .Select(net =>
                                        {
                                            netDict.TryGetValue(net.HardwareAddress ?? "", out var configNet);
                                            return new VmNetwork(item.Node,
                                                                 item.VmId,
                                                                 item.Name,
                                                                 item.Description,
                                                                 item.Type,
                                                                 item.Status,
                                                                 item.IsLocked,
                                                                 hostname,
                                                                 configNet == null,
                                                                 net.Name,
                                                                 net.HardwareAddress?.ToUpperInvariant(),
                                                                 configNet?.Bridge,
                                                                 configNet?.Tag,
                                                                 configNet?.Model,
                                                                 configNet?.Firewall ?? false,

                                                                 net.IpAddresses.Where(a => a.IpAddressType == "ipv4")
                                                                                .Select(a => $"{a.IpAddress}/{a.Prefix}")
                                                                                .JoinAsString(Environment.NewLine),

                                                                 net.IpAddresses.Where(a => a.IpAddressType == "ipv6")
                                                                                .Select(a => $"{a.IpAddress}/{a.Prefix}")
                                                                                .JoinAsString(Environment.NewLine),

                                                                 configNet?.Gateway,
                                                                 configNet?.Gateway6,
                                                                 configNet?.Mtu,
                                                                 configNet?.Rate);
                                        })];

                                case VmType.Lxc:
                                    var hostName = (config as VmConfigLxc)?.Hostname;
                                    return networks.Select(network => new VmNetwork(item.Node,
                                                                                    item.VmId,
                                                                                    item.Name,
                                                                                    item.Description,
                                                                                    item.Type,
                                                                                    item.Status,
                                                                                    item.IsLocked,
                                                                                    hostName,
                                                                                    false,
                                                                                    network.Id,
                                                                                    network.MacAddress,
                                                                                    network.Bridge,
                                                                                    network.Tag,
                                                                                    network.Model,
                                                                                    network.Firewall,
                                                                                    network.IpAddress,
                                                                                    network.IpAddress6,
                                                                                    network.Gateway,
                                                                                    network.Gateway6,
                                                                                    network.Mtu,
                                                                                    network.Rate));

                                default: return [];
                            }
                        }))
                        .OrderBy(a => a.Node)
                        .ThenBy(a => a.Type)
                        .ThenBy(a => a.VmId)];

        IsLoadingGuests = false;
        _guestsLoaded = true;
        await InvokeAsync(StateHasChanged);
    }
}
