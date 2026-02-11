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

    public static string RepoDockerHub { get; set; } = "corsinvest/cv4pve-admin";
    public static string RepoGitHub { get; } = "corsinvest/cv4pve-admin";
    public static bool IsEnterpriseEdition { get; set; }

    public static string UrlShopSubscription { get; } = "https://shop.corsinvest.it/store/cv4pve-admin-pve";
    public static string UrlNewPveConfig { get; set; } = default!;
    public static bool IsInContainer => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

    //public static string HelpUrl { get; } = "/doc/index.html";

    public static string ExecutionPath => Directory.GetCurrentDirectory();
    public static string DataPath => Path.Combine(ExecutionPath, "data");
    public static string ImagesPath => Path.Combine(DataPath, "images");
    public static string UserProfileImagesPath => Path.Combine(ImagesPath, "user-profile");
    public static string TempPath => Path.Combine(DataPath, "tmp");
    public static string ModulesPath => Path.Combine(DataPath, "modules");
    public static string FileNameSettings { get; } = "appsettings.json";
    public static string ModuleComponentUrl { get; } = "/module";

    //public static string GetGrAvatar(string email, string forceDefault = "retro")
    //{
    //    var hash = System.Security.Cryptography.MD5.HashData(Encoding.ASCII.GetBytes(email))
    //                                           .Select(a => a.ToString("X2"))
    //                                           .JoinAsString(string.Empty)
    //                                           .ToLower();
    //    return $"https://www.gravatar.com/avatar/{hash}?d={forceDefault}";
    //}
}
