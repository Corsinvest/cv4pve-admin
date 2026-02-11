using Microsoft.Extensions.Configuration;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.ReleaseChecker;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReleaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure named HttpClient for GitHub with timeout
        services.AddHttpClient("GitHubReleaseChecker", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
        });

        // Configure named HttpClient for Docker Hub with timeout
        services.AddHttpClient("DockerHubReleaseChecker", client => client.Timeout = TimeSpan.FromSeconds(30));

        services.AddScoped<IReleaseService, ReleaseService>();
        services.AddHostedService<ReleaseCheckHostedService>();

        return services;
    }
}
