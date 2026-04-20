/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.AppTokens;

public enum AppTokenValidationStatus
{
    Valid,
    NotFound,
    Inactive,
    Expired
}

public sealed record AppTokenValidationResult(AppTokenValidationStatus Status, AppToken? Token)
{
    public bool IsValid => Status == AppTokenValidationStatus.Valid;

    public string Describe() => Status switch
    {
        AppTokenValidationStatus.Valid => $"Token '{Token!.Name}' valid",
        AppTokenValidationStatus.NotFound => "Token not found",
        AppTokenValidationStatus.Inactive => $"Token '{Token!.Name}' is disabled",
        AppTokenValidationStatus.Expired => $"Token '{Token!.Name}' expired at {Token.ExpiresAt:yyyy-MM-dd HH:mm}",
        _ => "Unknown status"
    };
}
