/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class ApplicationHelper
{
    static ApplicationHelper()
    {
        Directory.CreateDirectory(DataPath);
        Directory.CreateDirectory(ModulesPath);
        Directory.CreateDirectory(TempPath);
        Directory.CreateDirectory(ImagesPath);
        Directory.CreateDirectory(UserProfileImagesPath);
    }

    public const string AllClusterName = "*";

    public const string DefaultAdminUsername = "admin@local";
    public const string DefaultAdminPassword = "Password123!";

    public static bool IsRunningInEfTool
        => AppDomain.CurrentDomain.FriendlyName?.Contains("ef", StringComparison.OrdinalIgnoreCase) == true
            || Environment.CommandLine.Contains("ef", StringComparison.OrdinalIgnoreCase);

    public const string ThemeName = "fluent";
    public const string CookieThemeName = "cv4pve-admin-theme";
    public const string RepoGitHub = "corsinvest/cv4pve-admin";
    public const string GitHubRepoUrl = $"https://github.com/{RepoGitHub}";
    public const string GitHubReleasesLatestDownloadUrl = $"{GitHubRepoUrl}/releases/latest/download";
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

    public const string UrlShopSubscription = "https://shop.corsinvest.it/store/cv4pve-admin-pve";
    public static string UrlNewPveConfig { get; set; } = default!;

    public static string ExecutionPath { get; } = AppContext.BaseDirectory;
    public static string ConfigPath { get; } = Path.Combine(ExecutionPath, "config");
    public static string DataPath { get; } = Path.Combine(ExecutionPath, "data");
    public static string ImagesPath { get; } = Path.Combine(DataPath, "images");
    public static string UserProfileImagesPath { get; } = Path.Combine(ImagesPath, "user-profile");
    public static string TempPath { get; } = Path.Combine(DataPath, "tmp");
    public static string ModulesPath { get; } = Path.Combine(DataPath, "modules");
    public const string ModuleComponentUrl = "/module/";
}
