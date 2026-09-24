/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class VmLxcInterfaceExtensions
{
    // Live interfaces keyed by MAC, to match them with the configured NICs.
    public static Dictionary<string, VmLxcInterface> ToDictionaryByMac(this IEnumerable<VmLxcInterface> interfaces)
        => interfaces.Where(a => !string.IsNullOrEmpty(a.MacAddress))
                     .GroupBy(a => a.MacAddress, StringComparer.OrdinalIgnoreCase)
                     .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

    // Addresses currently assigned (ip/prefix), link-local excluded.
    public static List<string> GetAddresses(this VmLxcInterface iface, bool ipv6)
    {
        var addresses = (iface.IpAddresses ?? [])
                            .Where(a => a.IpAddressType == (ipv6 ? "ipv6" : "ipv4") && !IsLinkLocal(a.IpAddress))
                            .Select(a => $"{a.IpAddress}/{a.Prefix}")
                            .ToList();

        var single = ipv6 ? iface.Inet6 : iface.Inet;
        if (addresses.Count == 0 && !string.IsNullOrEmpty(single) && !IsLinkLocal(single)) { addresses.Add(single); }

        return addresses;
    }

    // Config says "dhcp"/"auto"/"manual" (or nothing): show the address actually assigned, keeping the mode.
    public static string? GetLiveAddress(this VmLxcInterface? iface, string? configured, bool ipv6, string separator)
    {
        if (iface == null) { return configured; }

        var addresses = iface.GetAddresses(ipv6);
        if (addresses.Count == 0) { return configured; }

        var live = addresses.JoinAsString(separator);
        return configured is "dhcp" or "auto" or "manual"
                ? $"{live} ({configured})"
                : string.IsNullOrEmpty(configured) ? live : configured;
    }

    // Copy of the configured NIC with the live addresses; the original comes from the cache and must not change.
    public static VmNetwork WithLiveAddresses(this VmNetwork network, IReadOnlyDictionary<string, VmLxcInterface> live, string separator)
    {
        if (string.IsNullOrEmpty(network.MacAddress) || !live.TryGetValue(network.MacAddress, out var iface)) { return network; }

        return new VmNetwork
        {
            Id = network.Id,
            Name = network.Name,
            Bridge = network.Bridge,
            Type = network.Type,
            Queues = network.Queues,
            Tag = network.Tag,
            Firewall = network.Firewall,
            Gateway = network.Gateway,
            IpAddress = iface.GetLiveAddress(network.IpAddress, false, separator)!,
            IpAddress6 = iface.GetLiveAddress(network.IpAddress6, true, separator)!,
            Gateway6 = network.Gateway6,
            MacAddress = network.MacAddress,
            Model = network.Model,
            Rate = network.Rate,
            Disconnect = network.Disconnect,
            Trunks = network.Trunks,
            Mtu = network.Mtu,
            LinkDown = network.LinkDown,
            RawDefinition = network.RawDefinition
        };
    }

    private static bool IsLinkLocal(string? ip) => (ip ?? "").StartsWith("fe80", StringComparison.OrdinalIgnoreCase);
}
