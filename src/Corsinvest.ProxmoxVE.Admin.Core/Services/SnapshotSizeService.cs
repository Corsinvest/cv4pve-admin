namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

internal class SnapshotSizeService : ISnapshotSizeService
{
    public Task<IEnumerable<VmDiskInfo>> GetAsync(PveClient client) => Task.FromResult(Enumerable.Empty<VmDiskInfo>());
}
