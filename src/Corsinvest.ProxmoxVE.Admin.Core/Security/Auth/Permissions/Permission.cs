/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

public record Permission//(string Key, string Description)
{
    //public Permission(string parent, string subKey, string Description)
    //    : this($"{parent}.{subKey}", Description) { }

    public Permission(string parent, string subKey, string description)
    {
        Key = string.IsNullOrWhiteSpace(parent)
                ? subKey
                : $"{parent}.{subKey}";
        Description = description;
    }

    public string Key { get; }
    public string Description { get; }

    //public static Permission Create(string? description = null, [CallerMemberName] string name = "")
    //{
    //    var frame = new StackTrace().GetFrame(1);
    //    var method = frame!.GetMethod()!;
    //    var className = method.DeclaringType!.FullName!;
    //    var key = $"{className.Replace('+', '.')}.{name}";
    //    return new Permission(key, description ?? key);
    //}

    //public static Permission Create<T>(string? description = null, [CallerMemberName] string memberName = "")
    //{
    //    var key = $"{typeof(T).FullName!.Replace('+', '.')}.{memberName}";
    //    return new Permission(key, description ?? key);
    //}
}
