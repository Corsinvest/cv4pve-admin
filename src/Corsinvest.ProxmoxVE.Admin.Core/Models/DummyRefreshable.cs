namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public class DummyRefreshable : IRefreshableData
{
    public Task RefreshDataAsync() => Task.CompletedTask;
}
