/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.RegularExpressions;

namespace Corsinvest.ProxmoxVE.Admin.Module.Dashboard.Components.Widgets.WebContent;

public partial class RenderSettings : ISettingsParameter<Settings>
{
    [Parameter] public Settings Settings { get; set; } = default!;
    [Parameter] public EventCallback<Settings> SettingsChanged { get; set; }

    private void OnUrlChanged(string url) => Settings.Url = ConvertToEmbedUrl(url);

    private static string ConvertToEmbedUrl(string url)
    {
        var m = YouTubeRegex().Match(url);
        if (m.Success) { return $"https://www.youtube.com/embed/{m.Groups[1].Value}"; }

        m = YouTubeShortRegex().Match(url);
        if (m.Success) { return $"https://www.youtube.com/embed/{m.Groups[1].Value}"; }

        m = VimeoRegex().Match(url);
        if (m.Success) { return $"https://player.vimeo.com/video/{m.Groups[1].Value}"; }

        m = DailymotionRegex().Match(url);
        if (m.Success) { return $"https://www.dailymotion.com/embed/video/{m.Groups[1].Value}"; }

        m = LoomRegex().Match(url);
        if (m.Success) { return $"https://www.loom.com/embed/{m.Groups[1].Value}"; }

        m = GoogleDriveRegex().Match(url);
        if (m.Success) { return $"https://drive.google.com/file/d/{m.Groups[1].Value}/preview"; }

        m = GoogleSlidesRegex().Match(url);
        if (m.Success) { return $"https://docs.google.com/presentation/d/{m.Groups[1].Value}/embed"; }

        m = GoogleDocsRegex().Match(url);
        if (m.Success) { return $"https://docs.google.com/document/d/{m.Groups[1].Value}/preview"; }

        m = GoogleSheetsRegex().Match(url);
        if (m.Success) { return $"https://docs.google.com/spreadsheets/d/{m.Groups[1].Value}/preview"; }

        m = MicrosoftStreamRegex().Match(url);
        if (m.Success) { return $"https://web.microsoftstream.com/embed/video/{m.Groups[1].Value}"; }

        if (url.Contains("onedrive.live.com") && !url.Contains("embed"))
        {
            return $"{url.Replace("redir?", "embed?")}{(url.Contains('?') ? "&" : "?")}action=embedview";
        }

        if (url.Contains("sharepoint.com") && !url.Contains("embed"))
        {
            return $"{url}{(url.Contains('?') ? "&" : "?")}action=embedview";
        }

        return url;
    }

    [GeneratedRegex(@"youtube\.com/watch\?v=([^&]+)")]
    private static partial Regex YouTubeRegex();

    [GeneratedRegex(@"youtu\.be/([^?]+)")]
    private static partial Regex YouTubeShortRegex();

    [GeneratedRegex(@"vimeo\.com/(\d+)")]
    private static partial Regex VimeoRegex();

    [GeneratedRegex(@"dailymotion\.com/video/([^_?]+)")]
    private static partial Regex DailymotionRegex();

    [GeneratedRegex(@"loom\.com/share/([^?]+)")]
    private static partial Regex LoomRegex();

    [GeneratedRegex(@"drive\.google\.com/file/d/([^/]+)")]
    private static partial Regex GoogleDriveRegex();

    [GeneratedRegex(@"docs\.google\.com/presentation/d/([^/]+)")]
    private static partial Regex GoogleSlidesRegex();

    [GeneratedRegex(@"docs\.google\.com/document/d/([^/]+)")]
    private static partial Regex GoogleDocsRegex();

    [GeneratedRegex(@"docs\.google\.com/spreadsheets/d/([^/]+)")]
    private static partial Regex GoogleSheetsRegex();

    [GeneratedRegex(@"microsoftstream\.com/video/([^?]+)")]
    private static partial Regex MicrosoftStreamRegex();
}
