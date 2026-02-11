using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;
using SysFileInfo = System.IO.FileInfo;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Components.Monitoring;

public partial class SystemInfo(IReleaseService releaseService)
{
    private Dictionary<string, string> Info { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        var os = Environment.OSVersion;
        var process = Process.GetCurrentProcess();
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

        var latestRelease = await releaseService.NewReleaseIsAvailableAsync();
        var isInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

        // Get network info
        var hostName = Dns.GetHostName();
        var ipAddresses = string.Empty;
        try
        {
            var hostEntry = await Dns.GetHostEntryAsync(hostName);
            ipAddresses = string.Join("<br/>", hostEntry.AddressList.Select(ip => ip.ToString()));
        }
        catch
        {
            ipAddresses = "N/A";
        }

        Info = new Dictionary<string, string>
        {
            // App Info
            ["-- App Info --"] = string.Empty,
            ["App Version"] = Core.BuildInfo.Version,
            ["Edition"] = ApplicationHelper.IsEnterpriseEdition ? "Enterprise Edition" : "Community Edition",
            ["Installation Type"] = isInContainer ? "Container (Docker)" : "Executable",
            ["Latest Available Version"] = latestRelease?.Version ?? "Up to date",
            ["Auto-Update Supported"] = releaseService.IsAutoUpdateSupported ? "Yes" : "No",
            ["Framework"] = RuntimeInformation.FrameworkDescription,
            ["Runtime Version"] = Environment.Version.ToString(),
            ["Assembly Location"] = assembly.Location,
            ["Command Line Args"] = string.Join("<br/>", Environment.GetCommandLineArgs().Skip(1)),

            // Environment Variables
            ["-- Environment Variables --"] = string.Empty,
            ["ASPNETCORE_ENVIRONMENT"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "N/A",
            ["DOTNET_ENVIRONMENT"] = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "N/A",
            ["DOTNET_RUNNING_IN_CONTAINER"] = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") ?? "false",
            ["ASPNETCORE_URLS"] = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "N/A",
            ["ASPNETCORE_HTTP_PORTS"] = Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS") ?? "N/A",
            ["ASPNETCORE_HTTPS_PORTS"] = Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORTS") ?? "N/A",

            // Container Info (if running in container)
            ["-- Container Info --"] = string.Empty,
            ["Running in Container"] = isInContainer ? "Yes" : "No",

            // Memory Info
            ["-- Memory Info --"] = string.Empty,
            ["Process Working Set"] = ByteHelper.ToSizeString(process.WorkingSet64, false),
            ["GC Total Memory"] = ByteHelper.ToSizeString(GC.GetTotalMemory(forceFullCollection: false), false),
            ["GC Gen 0 Collections"] = GC.CollectionCount(0).ToString(),
            ["GC Gen 1 Collections"] = GC.CollectionCount(1).ToString(),
            ["GC Gen 2 Collections"] = GC.CollectionCount(2).ToString(),
            ["GC Max Generation"] = GC.MaxGeneration.ToString(),

            // OS Info
            ["-- OS Info --"] = string.Empty,
            ["OS Version"] = os.Version.ToString(),
            ["OS Platform"] = os.Platform.ToString(),
            ["OS Service Pack"] = string.IsNullOrEmpty(os.ServicePack) ? "None" : os.ServicePack,
            ["OS Version String"] = os.VersionString,
            ["OS Description"] = RuntimeInformation.OSDescription,
            ["OS Architecture"] = RuntimeInformation.OSArchitecture.ToString(),

            // Process Info
            ["-- Process Info --"] = string.Empty,
            ["Process Architecture"] = RuntimeInformation.ProcessArchitecture.ToString(),
            ["Process Uptime"] = (DateTime.UtcNow - process.StartTime.ToUniversalTime()).ToString(@"dd\.hh\:mm\:ss"),
            ["Process Memory"] = ByteHelper.ToSizeString(process.WorkingSet64, false),
            ["Process Managed Memory"] = ByteHelper.ToSizeString(GC.GetTotalMemory(forceFullCollection: false), false),
            ["Process CPU Usage %"] = await GetCpuUsageForProcessAsync(),
            ["Process CPU Time"] = Environment.CpuUsage.TotalTime.ToString(@"dd\.hh\:mm\:ss"),
            ["Processor Count"] = Environment.ProcessorCount.ToString(),

            // Machine & User
            ["-- Machine & User --"] = string.Empty,
            ["Machine Name"] = Environment.MachineName,
            ["Host Name"] = hostName,
            ["User Name"] = Environment.UserName,
            ["User Domain Name"] = Environment.UserDomainName,

            // Network Info
            ["-- Network Info --"] = string.Empty,
            ["IP Addresses"] = ipAddresses,

            // Time & Path
            ["-- Time --"] = string.Empty,
            ["Current Time (Local)"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            ["Current Time (UTC)"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            ["Time Zone"] = TimeZoneInfo.Local.DisplayName,
            ["Time Zone ID"] = TimeZoneInfo.Local.Id,
            ["Time Zone Offset"] = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).ToString(@"hh\:mm"),

            // Directory
            ["-- Path --"] = string.Empty,

            ["Current Directory"] = Environment.CurrentDirectory,
            ["Base Directory"] = AppContext.BaseDirectory,

            ["Images Path"] = ApplicationHelper.ImagesPath,
            ["Images Path Size"] = await GetDataPathSizeAsync(ApplicationHelper.ImagesPath),

            ["Temp Path"] = ApplicationHelper.TempPath,
            ["Temp Path Size"] = await GetDataPathSizeAsync(ApplicationHelper.TempPath),

            ["Data Path"] = ApplicationHelper.DataPath,
            ["Data Path Size"] = await GetDataPathSizeAsync(ApplicationHelper.DataPath)
        };
    }

    private static async Task<string> GetCpuUsageForProcessAsync()
    {
        var process = Process.GetCurrentProcess();
        var startTime = DateTime.UtcNow;
        var startCpuTime = process.TotalProcessorTime;

        await Task.Delay(1000);

        var endTime = DateTime.UtcNow;
        var endCpuTime = process.TotalProcessorTime;

        var cpuUsedMs = (endCpuTime - startCpuTime).TotalMilliseconds;
        var totalMsPassed = (endTime - startTime).TotalMilliseconds;

        var cpuUsageTotal = cpuUsedMs / (totalMsPassed * Environment.ProcessorCount) * 100;
        return cpuUsageTotal.ToString("F1");
    }

    private static async Task<long> GetDirectorySizeAsync(string path)
    {
        if (!Directory.Exists(path)) { return 0; }

        long size = 0;

        try
        {
            var fileSizesTasks = Directory.EnumerateFiles(path).Select(file => Task.Run(() => new SysFileInfo(file).Length));
            size += (await Task.WhenAll(fileSizesTasks)).Sum();

            foreach (var dir in Directory.EnumerateDirectories(path)) { size += await GetDirectorySizeAsync(dir); }
        }
        catch
        {
        }

        return size;
    }

    private static async Task<string> GetDataPathSizeAsync(string path) => $"{await GetDirectorySizeAsync(path) / (1024.0 * 1024.0):F2} MB";
}
