namespace Corsinvest.ProxmoxVE.Admin.Module.System.SystemLogs.Services;

internal class SystemLogService : ISystemLogService
{
    public Task<int> CleanupAsync(int retentionDays) => Task.FromResult(0);
}
