/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Prometheus;

namespace Corsinvest.ProxmoxVE.Admin.MetricsExporter;

public class Info
{
    public DateTime? LastRequest { get; set; }
    public long CountRequest { get; set; }
    public CollectorRegistry Registry { get; set; } = default!;
}
