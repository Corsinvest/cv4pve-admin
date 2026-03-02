/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.AIServer.Helpers;

/// <summary>
/// Output format for tool responses.
/// Add new values here to support additional formats without modifying ToolHelper.
/// </summary>
public enum ToolOutputFormat
{
    /// <summary>{"key":{"headers":[...],"rows":[[...],...]}} — compact JSON tabular, ~40% fewer tokens than JsonNormal</summary>
    [Display(Name = "JSON Compact")] JsonCompact,
    /// <summary>{"key":[{"field":value,...},...]} — standard JSON array</summary>
    [Display(Name = "JSON Normal")] JsonNormal,
    /// <summary>TOON (Token-Oriented Object Notation) — ~55% fewer tokens than JsonNormal</summary>
    Toon,
    /// <summary>CSV with header row — minimal tokens, best for purely tabular data</summary>
    Csv
}
