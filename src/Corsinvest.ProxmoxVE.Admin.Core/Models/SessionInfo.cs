/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public class SessionInfo
{
    public string UserName { get; set; } = default!;
    public string IpAddress { get; set; } = default!;
    public DateTime Login { get; set; }
    public DateTime LastActivity { get; set; }
    public string HttpConnectionId { get; set; } = default!;
    public string CircuitId { get; set; } = default!;
    public SessionStatus Status { get; set; } = SessionStatus.Online;
    public string? CurrentPage { get; set; }
    public string Browser { get; set; } = default!;
    public Version BrowserVersion { get; set; } = default!;
    public string Platform { get; set; } = default!;
    public Version PlatformVersion { get; set; } = default!;
    public string? Device { get; set; }
    public string UserAgent { get; set; } = default!;
    public string HubConnectionId { get; set; } = default!;
}
