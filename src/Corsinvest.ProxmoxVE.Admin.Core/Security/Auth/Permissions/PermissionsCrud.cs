/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

public record PermissionsCrud(string Prefix) : PermissionsRead(Prefix)
{
    public PermissionsCrud(params string[] prefix) : this(prefix.JoinAsString(".")) { }
    public Permission Create { get; } = new(Prefix, nameof(Create), "Create");
    public Permission Edit { get; } = new(Prefix, nameof(Edit), "Edit");
    public Permission Delete { get; } = new(Prefix, nameof(Delete), "Delete");

    private IEnumerable<Permission> _permissions = default!;
    public override IEnumerable<Permission> Permissions
        => _permissions ??= new[] { Create, Edit, Delete }.Union(base.Permissions);
}
