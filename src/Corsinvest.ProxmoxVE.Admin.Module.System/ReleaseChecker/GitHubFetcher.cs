/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.ReleaseChecker;

internal static class GitHubFetcher
{
    private const int MaxResponseSizeMB = 10;

    public static async Task<IEnumerable<ReleaseInfo>> GetReleasesAsync(
        IHttpClientFactory httpClientFactory,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ApplicationHelper.RepoGitHub) ||
            ApplicationHelper.RepoGitHub.Contains("..") ||
            ApplicationHelper.RepoGitHub.Contains("://"))
        {
            throw new InvalidOperationException("Invalid GitHub repository configuration");
        }

        var client = httpClientFactory.CreateClient("GitHubReleaseChecker");

        var version = string.IsNullOrWhiteSpace(Core.BuildInfo.Version) ? "unknown" : Core.BuildInfo.Version;
        client.DefaultRequestHeaders.UserAgent.Clear();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("cv4pve-admin", version));

        var url = $"https://api.github.com/repos/{ApplicationHelper.RepoGitHub}/releases";

        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (response.Content.Headers.ContentLength > MaxResponseSizeMB * 1024 * 1024)
        {
            throw new InvalidOperationException($"Response size exceeds {MaxResponseSizeMB}MB limit");
        }

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("GitHub API returned {StatusCode}: {Reason}", response.StatusCode, response.ReasonPhrase);
            return [];
        }

        var releases = await response.Content.ReadFromJsonAsync<List<GitHubRelease>>(cancellationToken: cancellationToken);

        return [.. (releases ?? [])
                   .Where(a => !a.Draft)
                   .Select(a => new ReleaseInfo
                   {
                       Prerelease = a.Prerelease,
                       Url = a.HtmlUrl ?? string.Empty,
                       PublishedAt = a.PublishedAt,
                       Version = a.TagName ?? string.Empty
                   })];
    }

    private sealed class GitHubRelease
    {
        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = default!;

        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = default!;

        [JsonPropertyName("draft")]
        public bool Draft { get; set; }

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; set; }

        [JsonPropertyName("published_at")]
        public DateTimeOffset PublishedAt { get; set; }
    }
}
