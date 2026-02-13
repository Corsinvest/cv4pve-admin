/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Semver;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.ReleaseChecker;

internal static class DockerHubFetcher
{
    private const int MaxResponseSizeMB = 10;

    public static async Task<IEnumerable<ReleaseInfo>> GetReleasesAsync(
        IHttpClientFactory httpClientFactory,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var repoDockerHub = Core.BuildInfo.RepoDockerHub;

        if (string.IsNullOrWhiteSpace(repoDockerHub)
            || repoDockerHub.Contains("..")
            || repoDockerHub.Contains("://"))
        {
            throw new InvalidOperationException("Invalid DockerHub repository configuration");
        }

        var client = httpClientFactory.CreateClient("DockerHubReleaseChecker");
        var url = $"https://registry.hub.docker.com/v2/repositories/{repoDockerHub}/tags";

        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (response.Content.Headers.ContentLength > MaxResponseSizeMB * 1024 * 1024)
        {
            throw new InvalidOperationException($"Response size exceeds {MaxResponseSizeMB}MB limit");
        }

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Docker Hub API returned {StatusCode}: {Reason}", response.StatusCode, response.ReasonPhrase);
            return [];
        }

        var data = await response.Content.ReadFromJsonAsync<DockerHubResponse>(cancellationToken: cancellationToken);

        return [.. (data?.Results ?? [])
                   .Select(tag =>
                   {
                       var isPrerelease = false;
                       if (!string.IsNullOrWhiteSpace(tag.Name) &&
                           SemVersion.TryParse(tag.Name, SemVersionStyles.Any, out var semVer))
                       {
                           isPrerelease = semVer.IsPrerelease;
                       }

                       return new ReleaseInfo
                       {
                           Prerelease = isPrerelease,
                           Url = $"https://hub.docker.com/r/{repoDockerHub}/tags",
                           PublishedAt = tag.TagLastPushed,
                           Version = tag.Name ?? string.Empty
                       };
                   })];
    }

    private sealed class DockerHubResponse
    {
        [JsonPropertyName("results")]
        public List<DockerHubTag> Results { get; set; } = [];
    }

    private sealed class DockerHubTag
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("tag_last_pushed")]
        public DateTimeOffset TagLastPushed { get; set; }
    }
}
