/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public class ResourceUsage
{
    public string Name { get; set; } = default!;
    public double Usage { get; set; }
    public string Info { get; set; } = default!;
    public string? Group { get; set; }
    public string Color => PveAdminUIHelper.GetColorRange(Usage);
}
