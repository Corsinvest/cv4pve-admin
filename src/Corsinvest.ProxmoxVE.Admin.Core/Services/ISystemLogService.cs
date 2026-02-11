namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface ISystemLogService
{
    Task<int> CleanupAsync(int retentionDays);
}
