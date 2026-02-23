/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.AppTokens;

public interface IAppTokenService
{
    /// <summary>
    /// Generates a new AppToken. Returns the raw token string (shown once) and the saved entity.
    /// </summary>
    Task<(string RawToken, AppToken Token)> GenerateAsync(string name, string? ownerId, DateTime? expiresAt);

    /// <summary>
    /// Validates a raw token string. Returns the AppToken if valid and active, null otherwise.
    /// </summary>
    Task<AppToken?> ValidateAsync(string rawToken);

    /// <summary>
    /// Revokes (deactivates) a token by id.
    /// </summary>
    Task RevokeAsync(Guid id);

    /// <summary>
    /// Deletes a token by id.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Updates name, owner, active status and expiration of an existing token.
    /// </summary>
    Task<bool> UpdateAsync(Guid id, string name, string? ownerId, bool isActive, DateTime? expiresAt);

    Task<List<AppToken>> GetAllAsync();
    Task<AppToken?> GetByIdAsync(Guid id);
}
