/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;

public class AppTokenPermission : BasePermission
{
    public Guid AppTokenId { get; set; }
    public virtual AppToken AppToken { get; set; } = default!;
}
