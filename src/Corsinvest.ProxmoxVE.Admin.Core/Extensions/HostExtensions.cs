using Microsoft.Extensions.Hosting;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class HostExtensions
{
    public static IServiceScope GetScopeFactory(this IHost host) => host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
    public static IConfiguration GetConfiguration(this IHost host) => host.Services.GetRequiredService<IConfiguration>();
}
