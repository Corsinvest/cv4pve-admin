/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.Extensions.Options;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Services;

// Permissions are read from the database by the permission service: in the session ticket they only add weight
internal class ApplicationClaimsPrincipalFactory(UserManager<ApplicationUser> userManager,
                                                 RoleManager<ApplicationRole> roleManager,
                                                 IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        foreach (var claim in identity.FindAll(ApplicationClaimTypes.Permission).ToList())
        {
            identity.RemoveClaim(claim);
        }
        return identity;
    }
}
