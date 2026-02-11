namespace Corsinvest.ProxmoxVE.Admin.Core.Commands;

public interface ICommand<TResult>
{
    string ClusterName { get; }
}
