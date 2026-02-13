/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

public record PermissionsRead(string Prefix)
{
    public PermissionsRead(params string[] prefix) : this(prefix.JoinAsString(".")) { }

    public Permission Read { get; } = new(Prefix, nameof(Read), "Read");
    public Permission Export { get; } = new(Prefix, nameof(Export), "Export");

    private IEnumerable<Permission> _permissions = default!;
    public virtual IEnumerable<Permission> Permissions => _permissions ??= [Read, Export];
}
