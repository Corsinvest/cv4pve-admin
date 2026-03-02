/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class HttpResponseExtensions
{
    public static void AppendCultureCookie(this HttpResponse response, string culture)
        => response.Cookies.Append(ApplicationHelper.CookieCultureName,
                                   CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                                   new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), SameSite = SameSiteMode.Lax });
}
