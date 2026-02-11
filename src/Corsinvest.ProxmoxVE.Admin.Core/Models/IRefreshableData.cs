namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public interface IRefreshableData
{
    static IRefreshableData Dummy = new DummyRefreshable();

    Task RefreshDataAsync();
}
