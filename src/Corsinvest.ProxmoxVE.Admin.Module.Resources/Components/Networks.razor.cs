/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */

using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources.Components;

public partial class Networks(IAdminService adminService) : IClusterName, IRefreshableData
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private List<VmNetwork> VmNetworks { get; set; } = [];
    private List<NodeNetwork> NodeNetworks { get; set; } = [];
    private bool IsLoadingNodes { get; set; }
    private bool IsLoadingGuests { get; set; }
    private RadzenDataGrid<VmNetwork> DataGridRef { get; set; } = default!;
    private int SelectedTab { get; set; }
    private bool _nodesLoaded;
    private bool _guestsLoaded;

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
        if (SelectedTab == 0)
        {
            _nodesLoaded = false;
            await LoadNoteNetworksAsync();
        }
        else
        {
            _guestsLoaded = false;
            await LoadVmNetworksAsync();
        }
    }

    public async Task OnTabChangeAsync(int index)
    {
        SelectedTab = index;
        if (index == 0 && !_nodesLoaded)
        {
            await LoadNoteNetworksAsync();
        }
        else if (index == 1 && !_guestsLoaded)
        {
            await LoadVmNetworksAsync();
        }
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
