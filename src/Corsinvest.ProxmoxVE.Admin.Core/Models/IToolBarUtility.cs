namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public interface IToolBarUtility<T>
{
    Task ExecuteAsync(string clusterName, T item);
    string Icon { get; }
    bool IsVIsible(T item);
    bool RequireConfirm { get; }
    string Text { get; }
    ToolBarUtilityType Type { get; }
}
