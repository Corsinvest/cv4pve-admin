/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Encodings.Web;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.AppTokens;
using Microsoft.Extensions.Options;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security;

public class AppTokenAuthenticationOptions : AuthenticationSchemeOptions { }

public class AppTokenAuthenticationHandler(
    IOptionsMonitor<AppTokenAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IAppTokenService appTokenService)
    : AuthenticationHandler<AppTokenAuthenticationOptions>(options, logger, encoder)
{
    public const string SchemeName = "AppToken";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            return AuthenticateResult.NoResult();
        }

        var header = authHeader.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var rawToken = header["Bearer ".Length..].Trim();
        if (string.IsNullOrEmpty(rawToken)) { return AuthenticateResult.NoResult(); }

        var token = await appTokenService.ValidateAsync(rawToken);
        if (token == null) { return AuthenticateResult.Fail("Invalid or expired token."); }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, token.Id.ToString()),
            new Claim(ClaimTypes.Name, token.Name),
            new Claim(ApplicationClaimTypes.AppTokenId, token.Id.ToString())
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return AuthenticateResult.Success(ticket);
    }
}
