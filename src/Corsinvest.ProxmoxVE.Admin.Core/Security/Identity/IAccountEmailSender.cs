/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.AspNetCore.Identity;

namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

public interface IAccountEmailSender : IEmailSender<ApplicationUser>
{
    Task SendTwoFactorDisabledAsync(ApplicationUser user, bool byAdministrator);
}
