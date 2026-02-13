/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;

public class AuditLog
{
    public long Id { get; set; }
    public string? UserId { get; set; }
    public string UserName { get; set; } = default!;
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = default!;
    public bool Success { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public ApplicationUser? User { get; set; }
}
