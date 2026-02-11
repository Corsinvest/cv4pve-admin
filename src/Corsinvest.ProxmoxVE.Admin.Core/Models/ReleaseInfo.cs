namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public class ReleaseInfo
{
    public string? Url { get; set; }
    public bool Prerelease { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public string Version { get; set; } = default!;
    //public SemVersion SemVer => SemVersion.Parse(Version, SemVersionStyles.Any);
}
