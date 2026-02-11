using System.ComponentModel.DataAnnotations.Schema;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Schema;

/// <summary>
/// Qemu VM configuration information
/// </summary>
[Table("qemuConfigs")]
[Description("Qemu Config")]
public class QemuConfigInfo
{
    private readonly Lazy<VmConfigQemu> _dataLazy;
    private readonly ClusterCachedData _cachedData;

    public QemuConfigInfo(long guestId, string nodeName, ClusterCachedData cachedData)
    {
        GuestId = guestId;
        NodeName = nodeName;
        _cachedData = cachedData;
        _dataLazy = new Lazy<VmConfigQemu>(LoadData);
    }

    public long GuestId { get; }

    public string NodeName { get; }

    public int NumberOfCores => _dataLazy.Value.Cores;

    public bool StartOnBootEnabled => _dataLazy.Value.OnBoot;

    [Field(AllowedValues = ["x86_64", "aarch64"])]
    public string ProcessorArchitecture => _dataLazy.Value.Arch;

    public bool AcpiSupported => _dataLazy.Value.Acpi;

    public string EmulatedCPUtype => _dataLazy.Value.Cpu;

    public bool AgentEnabled => _dataLazy.Value.AgentEnabled;

    public string AgentConfig => _dataLazy.Value.Agent;

    public string OrderStartUp => _dataLazy.Value.StartUp;

    public int NumberOfSockets => _dataLazy.Value.Sockets;

    [Field(Description = "Amount of target RAM for the VM in MB. Using zero disables the ballon driver.")]
    public int BalloonRam => _dataLazy.Value.Balloon;

    [Field(AllowedValues = ["seabios", "ovmf"])]
    public string BiosType => _dataLazy.Value.Bios;

    public bool KvmHardwareVirtualizationEnable => _dataLazy.Value.Kvm;

    public bool ConfigurationProtected => _dataLazy.Value.Protection;

    public string SnapshotParent => _dataLazy.Value.Parent;

    public int SharesMemory => _dataLazy.Value.Shares;

    [Field(AllowedValues = ["other => Unspecified OS",
                           "w2k => Microsoft Windows 2000",
                           "wxp => Microsoft Windows XP/2003",
                           "w2k8 => Microsoft Windows Vista/2008",
                           "win7 => Microsoft Windows 7/2008r2",
                           "win8 => Microsoft Windows 8.x/2012/2012r2",
                           "win10 => Microsoft Windows 10/2016/2019",
                           "win11 => Microsoft Windows 11/2022/2025",
                           "l24 => Linux 2.4 Kernel",
                           "l26 => Linux 6.x - 2.6 Kernel",
                           "solaris => Solaris/OpenSolaris/OpenIndiania Kernel"])]
    public string OperationSystemType => _dataLazy.Value.OsType;

    private VmConfigQemu LoadData()
    {
#if DEBUG
        Console.WriteLine($"Load Qemu config - GuestId: {GuestId}, Node: {NodeName}");
#endif
        return AsyncHelper.RunSync(() => _cachedData.GetQemuConfigAsync(NodeName, GuestId, false));
    }
}
