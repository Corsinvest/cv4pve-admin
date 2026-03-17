/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.RegularExpressions;

namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static partial class ApplicationHelper
{
    static ApplicationHelper()
    {
        Directory.CreateDirectory(DataPath);
        Directory.CreateDirectory(ModulesPath);
        Directory.CreateDirectory(TempPath);
        Directory.CreateDirectory(ImagesPath);
        Directory.CreateDirectory(UserProfileImagesPath);
        Directory.CreateDirectory(TranslationsPath);
    }

    public const string AllClusterName = "*";
    public static bool IsAllCluster(string? clusterName) => clusterName == AllClusterName;

    [GeneratedRegex(@"^[a-zA-Z0-9_-]+$")]
    private static partial Regex ClusterNameRegex();

    public static bool IsValidClusterName(string? name)
        => !string.IsNullOrWhiteSpace(name)
           && name != AllClusterName
           && ClusterNameRegex().IsMatch(name);

    public const string DefaultCulture = "en";
    public static string[] SupportedCultures => [DefaultCulture];

    public const string DefaultAdminUsername = "admin@local";
    public const string DefaultAdminPassword = "Password123!";

    public static bool IsRunningInEfTool
        => AppDomain.CurrentDomain.FriendlyName?.Contains("ef", StringComparison.OrdinalIgnoreCase) == true
            || Environment.CommandLine.Contains("ef", StringComparison.OrdinalIgnoreCase);

    public const string ThemeName = "fluent";
    public const string CookieThemeName = "cv4pve-admin-theme";
    public const string CookieCultureName = "cv4pve-admin-culture";
    public const string RepoGitHub = "corsinvest/cv4pve-admin";
    public const string GitHubRepoUrl = $"https://github.com/{RepoGitHub}";
    public static string GitHubReleasesVersionDownloadUrl => $"{GitHubRepoUrl}/releases/download/v{BuildInfo.Version}";
    public const string DocumentationUrl = "https://corsinvest.github.io/cv4pve-admin";
    private const string GitHubIssueBaseUrl = $"{GitHubRepoUrl}/issues/new";

    public static string GetBugReportUrl(string environment)
        => $"{GitHubIssueBaseUrl}?template=bug_report.yml" +
           $"&edition={BuildInfo.EditionFull} ({BuildInfo.Edition})" +
           $"&version={BuildInfo.Version}" +
           $"&environment={Uri.EscapeDataString(environment)}";

    public static string FeatureRequestUrl => $"{GitHubIssueBaseUrl}?template=feature_request.yml";

    public static string FeedbackUrl
        => $"{GitHubIssueBaseUrl}?template=feedback.yml" +
           $"&edition={BuildInfo.EditionFull} ({BuildInfo.Edition})" +
           $"&version={BuildInfo.Version}";

    public static string GetWhoIsUsingUrl(string body)
        => $"{GitHubIssueBaseUrl}?title={Uri.EscapeDataString("Who's using cv4pve-admin?")}" +
           $"&body={Uri.EscapeDataString(body)}";

    public const string ShopSubscriptionUrl = "https://shop.corsinvest.it/store/cv4pve-admin-pve";

    public static string ExecutionPath { get; } =
#if DEBUG
        Directory.GetCurrentDirectory();
#else
        AppContext.BaseDirectory;
#endif
    public static string ConfigPath { get; } = Path.Combine(ExecutionPath, "config");
    public static string DataPath { get; } = Path.Combine(ExecutionPath, "data");
    public static string ImagesPath { get; } = Path.Combine(DataPath, "images");
    public static string UserProfileImagesPath { get; } = Path.Combine(ImagesPath, "user-profile");
    public static string TempPath { get; } = Path.Combine(DataPath, "tmp");
    public static string TranslationsPath { get; } = Path.Combine(DataPath, "translations");
    public static string ModulesPath { get; } = Path.Combine(DataPath, "modules");
}
