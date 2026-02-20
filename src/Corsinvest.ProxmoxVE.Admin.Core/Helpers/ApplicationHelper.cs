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

    public static string AllClusterName { get; } = "*";

    public static bool IsRunningInEfTool
        => AppDomain.CurrentDomain.FriendlyName?.Contains("ef", StringComparison.OrdinalIgnoreCase) == true
            || Environment.CommandLine.Contains("ef", StringComparison.OrdinalIgnoreCase);

    public static string ThemeName { get; } = "fluent";
    public static string CookieThemeName { get; } = "cv4pve-admin-theme";
    public static string RepoGitHub { get; } = "corsinvest/cv4pve-admin";

    private static string GitHubIssueBaseUrl { get; } = $"https://github.com/{RepoGitHub}/issues/new";

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

    public static string UrlShopSubscription { get; } = "https://shop.corsinvest.it/store/cv4pve-admin-pve";
    public static string UrlNewPveConfig { get; set; } = default!;
    public static bool IsInContainer => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

    //public static string HelpUrl { get; } = "/doc/index.html";

    public static string ExecutionPath => AppContext.BaseDirectory;
    public static string ConfigPath => Path.Combine(ExecutionPath, "config");
    public static string DataPath => Path.Combine(ExecutionPath, "data");
    public static string ImagesPath => Path.Combine(DataPath, "images");
    public static string UserProfileImagesPath => Path.Combine(ImagesPath, "user-profile");
    public static string TempPath => Path.Combine(DataPath, "tmp");
    public static string ModulesPath => Path.Combine(DataPath, "modules");
    public static string FileNameSettings { get; } = "appsettings.json";
    public static string ModuleComponentUrl { get; } = "/module";
}
