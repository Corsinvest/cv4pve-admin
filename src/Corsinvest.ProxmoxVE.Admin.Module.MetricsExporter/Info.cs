/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Prometheus;

namespace Corsinvest.ProxmoxVE.Admin.Module.MetricsExporter;

internal class Info
{
    private long _countRequest;
    private long _lastRequestTicks;

    public DateTime? LastRequest
        => Interlocked.Read(ref _lastRequestTicks) is var ticks and > 0
            ? new DateTime(ticks, DateTimeKind.Local)
            : null;

    public long CountRequest => Interlocked.Read(ref _countRequest);
    public CollectorRegistry Registry { get; set; } = default!;

    public void RegisterRequest()
    {
        Interlocked.Exchange(ref _lastRequestTicks, DateTime.Now.Ticks);
        Interlocked.Increment(ref _countRequest);
    }
}
