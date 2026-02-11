namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface ISnapshotSizeService
{
    Task<IEnumerable<VmDiskInfo>> GetAsync(PveClient client);
}
