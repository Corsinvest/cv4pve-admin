/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class FormatterHelper
{
    //public const string FormatBytes = "{0:" + FormatHelper.DataFormatBytes + "}";
    //public const string FormatUnixTime = "{0:" + FormatHelper.FormatUptimeUnixTime + "}";

    private static readonly Lazy<PveFormatProvider> _formatProvider = new();
    public static PveFormatProvider FormatProvider { get; } = _formatProvider.Value;

    /// <summary>
    /// Human-friendly duration: "12 sec", "3 min 5 sec", "2h 14 min".
    /// Suitable for notification bodies, audit logs and task summaries.
    /// </summary>
    public static string FormatDuration(TimeSpan d)
        => d.TotalMinutes < 1
            ? $"{d.Seconds} sec"
            : d.TotalHours < 1
                ? $"{d.Minutes} min {d.Seconds} sec"
                : $"{(int)d.TotalHours}h {d.Minutes} min";
}
