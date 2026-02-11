namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface ICurrentClusterService
{
    string? ClusterName { get; set; }
}

internal class CurrentClusterService : ICurrentClusterService
{
    private static readonly AsyncLocal<string?> _clusterName = new();

    public string? ClusterName
    {
        get => _clusterName.Value;
        set => _clusterName.Value = value;
    }
}
