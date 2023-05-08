/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Configurations;

public class AdminOptions
{
    public List<ClusterOptions> Clusters { get; set; } = new();

    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Company { get; set; } = default!;
    public string Email { get; set; } = default!;
}
