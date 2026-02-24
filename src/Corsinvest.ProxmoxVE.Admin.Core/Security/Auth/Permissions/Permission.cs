/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

public record Permission
{
    public Permission(string parent, string subKey, string description)
    {
        Key = string.IsNullOrWhiteSpace(parent)
                ? subKey
                : $"{parent}.{subKey}";
        Description = description;
    }

    public string Key { get; }
    public string Description { get; }
}
