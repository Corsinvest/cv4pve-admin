namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface ISessionsInfoTracker
{
    IEnumerable<SessionInfo> Sessions { get; }
    event EventHandler OnChanged;
    void UpdateCurrentPage(string circuitId, string currentPage);
    void SetHubConnectionId(string circuitId, string hubConnectionId);
}
