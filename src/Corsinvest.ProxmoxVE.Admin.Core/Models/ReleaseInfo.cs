/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public class ReleaseInfo
{
    public string? Url { get; set; }
    public bool Prerelease { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public string Version { get; set; } = default!;
    //public SemVersion SemVer => SemVersion.Parse(Version, SemVersionStyles.Any);
}
