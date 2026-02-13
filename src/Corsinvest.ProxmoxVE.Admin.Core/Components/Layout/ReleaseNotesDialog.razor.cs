/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Reflection;
using System.Text;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Layout;

public partial class ReleaseNotesDialog
{
    private string ChangelogContent { get; set; } = string.Empty;

    protected override void OnInitialized() => ReadEditionSection();

    private void ReadEditionSection()
    {
        var version = BuildInfo.Version;
        var assembly = Assembly.GetEntryAssembly()!;
        using var stream = assembly.GetManifestResourceStream("Corsinvest.ProxmoxVE.Admin.CHANGELOG.md")!;
        using var reader = new StreamReader(stream);

        var sb = new StringBuilder();
        var inVersion = false;

        string line;
        while ((line = reader.ReadLine()!) != null)
        {
            if (line.StartsWith("## [") && line.Contains(version))
            {
                inVersion = true;
                continue;
            }

            if (inVersion && line.StartsWith("## [")) { break; }

            if (inVersion) { sb.AppendLine(line); }
        }

        ChangelogContent = sb.ToString().Trim();
    }
}
