/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Security.Claims;

namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    Task<ApplicationUser?> GetUserAsync();
    ClaimsPrincipal? ClaimsPrincipal { get; }
    string UserId { get; }
    string UserName { get; }
    string IpAddress { get; }
    string HttpConnectionId { get; }
    string CircuitId { get; }
}
