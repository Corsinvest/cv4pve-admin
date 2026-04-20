/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class SensitiveDataHelper
{
    public const string Mask = "***";

    private static readonly HashSet<string> SensitiveKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "passwd",
        "pwd",
        "token",
        "apikey",
        "secret",
        "credentials",
        "credential",
        "privatekey"
    };

    public static bool IsSensitive(string? fieldName)
    {
        if (string.IsNullOrEmpty(fieldName)) { return false; }
        var normalized = fieldName.Replace("_", string.Empty).Replace("-", string.Empty);
        foreach (var keyword in SensitiveKeywords)
        {
            if (normalized.Contains(keyword, StringComparison.OrdinalIgnoreCase)) { return true; }
        }
        return false;
    }

    public static string MaskValue(string? fieldName, string? value)
        => IsSensitive(fieldName) ? Mask : (value ?? string.Empty);

    public static string FormatPairs(IEnumerable<KeyValuePair<string, string?>> pairs)
        => string.Join(", ", pairs.Select(p => $"{p.Key}={MaskValue(p.Key, p.Value)}"));
}
