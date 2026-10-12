/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

// A sign-in that is still valid. The cookie of the browser holds the key, the ticket with the identity is here,
// so that a session survives a restart and the sessions of a user can be closed from the server.
public class UserSession
{
    [Key] public string Key { get; set; } = default!;

    [Required] public string UserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    // The authentication ticket, protected as the cookie would be
    public byte[] Ticket { get; set; } = default!;
}
