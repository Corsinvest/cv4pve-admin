/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class StringExtension
{
    public static bool IsNullOrEmptyOrWhiteSpace(this string value)
        => string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value);
}
