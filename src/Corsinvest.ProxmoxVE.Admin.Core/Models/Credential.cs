/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public class Credential
{
    public string Username { get; set; } = default!;

    [Encrypt]
    public string Password { get; set; } = default!;
}
