/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Reflection;

namespace Corsinvest.ProxmoxVE.Admin.Core;

public static class BuildInfo
{
    public static readonly string Version;
    public static readonly string PreRelease;
    public static readonly bool IsTesting;
    public const int TestingExpirationMonths = 3;

    public static readonly string Edition;
    public static readonly bool IsEnterpriseEdition;
    public static readonly string EditionFull;
    public static readonly string RepoDockerHub;
    public static readonly DateTime BuildDate;
    public static readonly bool IsTestingExpired;

    static BuildInfo()
    {
        if (Helpers.ApplicationHelper.IsRunningInEfTool) { return; }

        var assembly = Assembly.GetEntryAssembly()!;

        Version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion.Split('+')[0];

        // Extract pre-release from version (e.g., "2.0.0-rc1" -> "rc1")
        var parts = Version.Split('-', 2);
        PreRelease = parts.Length > 1
                        ? parts[1]
                        : string.Empty;

        // IsTesting is true if there's a pre-release suffix
        IsTesting = !string.IsNullOrEmpty(PreRelease);

        // Read Edition from EditionAttribute
        Edition = assembly.GetCustomAttribute<EditionAttribute>()!.Edition;
        IsEnterpriseEdition = Edition == "EE";

        // Compute derived properties
        EditionFull = IsEnterpriseEdition
                        ? "Enterprise Edition"
                        : "Community Edition";

        RepoDockerHub = IsEnterpriseEdition
                            ? "corsinvest/cv4pve-admin-ee"
                            : "corsinvest/cv4pve-admin";

        BuildDate = File.GetLastWriteTimeUtc(assembly.Location);
        IsTestingExpired = IsTesting && DateTime.UtcNow > BuildDate.AddMonths(TestingExpirationMonths);
    }
}
