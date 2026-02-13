/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.ComponentModel.DataAnnotations.Schema;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Schema;

/// <summary>
/// LXC container configuration information
/// </summary>
[Table("lxcConfigs")]
[Description("Lxc Config")]
public class LxcConfigInfo
{
    private readonly Lazy<VmConfigLxc> _dataLazy;
    private readonly ClusterCachedData _cachedData;

    public LxcConfigInfo(long guestId, string nodeName, ClusterCachedData cachedData)
    {
        GuestId = guestId;
        NodeName = nodeName;
        _cachedData = cachedData;
        _dataLazy = new Lazy<VmConfigLxc>(LoadData);
    }

    public long GuestId { get; }

    public string NodeName { get; }

    public int NumberOfCores => _dataLazy.Value.Cores;

    public bool StartOnBootEnabled => _dataLazy.Value.OnBoot;

    [Field(AllowedValues = ["x86_64", "aarch64"])]
    public string ProcessorArchitecture => _dataLazy.Value.Arch;

    public bool ConfigurationProtected => _dataLazy.Value.Protection;

    public string SnapshotParent => _dataLazy.Value.Parent;

    [Field(AllowedValues = ["debian", "devuan", "ubuntu", "centos", "fedora", "opensuse", "archlinux", "alpine", "gentoo", "nixos", "unmanaged"])]
    public string OperationSystemType => _dataLazy.Value.OsType;

    private VmConfigLxc LoadData()
    {
#if DEBUG
        Console.WriteLine($"Load Lxc config - GuestId: {GuestId}, Node: {NodeName}");
#endif
        return AsyncHelper.RunSync(() => _cachedData.GetLxcConfigAsync(NodeName, GuestId, false));
    }
}
