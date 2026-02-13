/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Reflection;
using PdfSharp.Fonts;

namespace Corsinvest.ProxmoxVE.Admin.Core.Fonts;

public class PdfSharpEmbeddedFontResolver : IFontResolver
{
    public FontResolverInfo ResolveTypeface(string familyName, bool bold, bool italic)
    {
        var fontKey = familyName.ToLowerInvariant() switch
        {
            "arial" or "sans-serif" or "helvetica" =>
                bold ? "DejaVuSans-Bold" : "DejaVuSans",
            "times new roman" or "times" or "serif" =>
                bold ? "DejaVuSerif-Bold" : "DejaVuSerif",
            "courier new" or "courier" or "monospace" or "consolas" =>
                "DejaVuSansMono",
            _ => "DejaVuSans"
        };

        return new FontResolverInfo(fontKey);
    }

    public byte[] GetFont(string faceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream($"{GetType().Namespace}.{faceName}.ttf")
                            ?? throw new FileNotFoundException($"Font '{faceName}' not found. " +
                                                               $"Resource available: {assembly.GetManifestResourceNames().JoinAsString(", ")}");
        var fontData = new byte[stream.Length];
        stream.ReadExactly(fontData, 0, (int)stream.Length);
        return fontData;
    }
}
