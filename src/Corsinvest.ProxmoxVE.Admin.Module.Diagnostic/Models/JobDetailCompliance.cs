/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Diagnostic.Api.Compliance;

namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Models;

public class JobDetailCompliance : IId
{
    public int Id { get; set; }
    [Required] public JobDetail JobDetail { get; set; } = default!;
    public ComplianceStandard Standard { get; set; }
    public string ControlId { get; set; } = default!;
}
