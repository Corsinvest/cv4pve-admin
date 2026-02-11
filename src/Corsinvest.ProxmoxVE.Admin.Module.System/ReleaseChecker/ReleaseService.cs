using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Semver;
using ZiggyCreatures.Caching.Fusion;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.ReleaseChecker;

public class ReleaseService(IHttpClientFactory httpClientFactory,
                            ILogger<ReleaseService> logger,
                            IConfiguration configuration,
                            IFusionCache fusionCache) : IReleaseService
{
    private const string CacheKey = "LatestReleaseInfo";
    private string WatchtowerUrl => configuration["Container:Watchtower:Url"]!;
    private string WatchtowerToken => configuration["Container:Watchtower:Token"]!;
    private bool UseDockerHub => !string.IsNullOrWhiteSpace(WatchtowerUrl);

    public event EventHandler<ReleaseInfo>? NewReleaseDiscovered;

    public bool IsAutoUpdateSupported => UseDockerHub && !Core.BuildInfo.IsTesting;

    public async Task<IEnumerable<ReleaseInfo>> GetReleasesAsync(CancellationToken cancellationToken = default)
        => UseDockerHub
            ? await DockerHubFetcher.GetReleasesAsync(httpClientFactory, logger, cancellationToken)
            : await GitHubFetcher.GetReleasesAsync(httpClientFactory, logger, cancellationToken);

    public async Task<ReleaseInfo?> NewReleaseIsAvailableAsync(bool includePrerelease = false, bool force = false, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKey}:{includePrerelease}";

        if (force) { await fusionCache.RemoveAsync(cacheKey, token: cancellationToken); }

        return await fusionCache.GetOrSetAsync(cacheKey,
                                               async ct =>
                                               {
                                                   var newRelease = await GetLatestReleaseAsync(includePrerelease, ct);
                                                   if (newRelease != null) { NewReleaseDiscovered?.Invoke(this, newRelease!); }

                                                   return newRelease;
                                               },
                                               TimeSpan.FromHours(12),
                                               token: cancellationToken
        );
    }

    private async Task<ReleaseInfo?> GetLatestReleaseAsync(bool includePrerelease, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Core.BuildInfo.Version))
        {
            logger.LogWarning("Application version is null or empty");
            return null;
        }

        if (!SemVersion.TryParse(Core.BuildInfo.Version, SemVersionStyles.Any, out var currentVersion))
        {
            logger.LogWarning("Unable to parse current application version: {Version}", Core.BuildInfo.Version);
            return null;
        }

        IEnumerable<ReleaseInfo>? releases = null;
        try
        {
            releases = await GetReleasesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching releases");
            return null;
        }

        var orderedReleases = (releases ?? [])
            .Where(r => r.PublishedAt.HasValue)
            .OrderByDescending(r => r.PublishedAt!.Value);

        foreach (var release in orderedReleases)
        {
            if (release.Prerelease && !includePrerelease) { continue; }

            if (!SemVersion.TryParse(release.Version, SemVersionStyles.Any, out var releaseVersion))
            {
                logger.LogWarning("Unable to parse release version: {Version}", release.Version);
                continue;
            }

            if (currentVersion.ComparePrecedenceTo(releaseVersion) < 0)
            {
                return release;
            }
        }

        return null;
    }

    public async Task<bool> TriggerUpdateAsync(CancellationToken cancellationToken = default)
    {
        if (!UseDockerHub)
        {
            throw new NotSupportedException("Update trigger is only supported when running in Docker with Watchtower");
        }

        if (string.IsNullOrWhiteSpace(WatchtowerUrl))
        {
            logger.LogError("Watchtower URL is not configured");
            return false;
        }

        try
        {
            using var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            // Add Bearer token authentication if configured
            if (!string.IsNullOrWhiteSpace(WatchtowerToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", WatchtowerToken);
            }

            var response = await client.GetAsync($"{WatchtowerUrl}/v1/update", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Update triggered successfully via Watchtower");
                return true;
            }

            logger.LogWarning("Failed to trigger update via Watchtower. Status: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error triggering update via Watchtower");
            return false;
        }
    }
}
