/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Hooks;

public class WebHookResult
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public string? ErrorMessage { get; set; }
    public long DurationMs { get; set; }
}
