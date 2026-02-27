/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Reflection;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        if (member == null) { return value.ToString().SplitCamelCase(); }

        var display = member.GetCustomAttribute<DisplayAttribute>();
        if (!string.IsNullOrWhiteSpace(display?.GetName())) { return display.GetName()!; }

        var description = member.GetCustomAttribute<DescriptionAttribute>();
        if (!string.IsNullOrWhiteSpace(description?.Description)) { return description.Description; }

        return value.ToString().SplitCamelCase();
    }
}
