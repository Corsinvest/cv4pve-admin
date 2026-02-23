/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

public class AppToken
{
    public Guid Id { get; set; }

    [Required] public string Name { get; set; } = default!;

    /// <summary>SHA-256 hash of the raw token — never store the raw value.</summary>
    [Required] public string TokenHash { get; set; } = default!;

    public string? OwnerId { get; set; }
    public virtual ApplicationUser? Owner { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    public virtual ICollection<AppTokenRole> AppTokenRoles { get; set; } = [];
    public virtual ICollection<AppTokenPermission> AppTokenPermissions { get; set; } = [];

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    public bool IsValid => IsActive && !IsExpired;
}
