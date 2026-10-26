/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.AspNetCore.Identity;

namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

public enum ExternalLoginDisplay
{
    IconOnly,
    IconAndTitle,
    TitleOnly
}

public record ExternalLoginButton(string Scheme, string Name, string Icon, string? Description, ExternalLoginDisplay Display);

public record ExternalSignInResult(bool Succeeded, ApplicationUser? User, string? FailureReason, TimeSpan? SessionLifetime = null)
{
    public static ExternalSignInResult Fail(string? reason = null) => new(false, null, reason);
}

// Sign in with identity providers and domains configured in the application
public interface IExternalAuthenticationService
{
    Task<IReadOnlyList<ExternalLoginButton>> GetLoginButtonsAsync();
    Task<IReadOnlyList<string>> GetDomainNamesAsync();

    // Null when no domain provider handles this user name
    Task<ExternalSignInResult?> TryDomainSignInAsync(string userName, string password, string? domain);

    Task<ExternalSignInResult> HandleCallbackAsync(ExternalLoginInfo info);
}

public class NullExternalAuthenticationService : IExternalAuthenticationService
{
    public Task<IReadOnlyList<ExternalLoginButton>> GetLoginButtonsAsync() => Task.FromResult<IReadOnlyList<ExternalLoginButton>>([]);
    public Task<IReadOnlyList<string>> GetDomainNamesAsync() => Task.FromResult<IReadOnlyList<string>>([]);
    public Task<ExternalSignInResult?> TryDomainSignInAsync(string userName, string password, string? domain) => Task.FromResult<ExternalSignInResult?>(null);
    public Task<ExternalSignInResult> HandleCallbackAsync(ExternalLoginInfo info) => Task.FromResult(ExternalSignInResult.Fail());
}
