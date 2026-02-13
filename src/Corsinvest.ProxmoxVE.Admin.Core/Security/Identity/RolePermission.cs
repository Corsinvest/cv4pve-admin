/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

public class RolePermission : BasePermission
{
    public string RoleId { get; set; } = default!;
    [Required] public ApplicationRole Role { get; set; } = default!;
}
