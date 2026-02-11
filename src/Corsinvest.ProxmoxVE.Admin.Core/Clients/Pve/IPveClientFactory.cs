namespace Corsinvest.ProxmoxVE.Admin.Core.Clients.Pve;

public interface IPveClientFactory
{
    Task<PveClient> CreateClientAsync(ClusterSettings settings, CancellationToken cancellationToken);
}
