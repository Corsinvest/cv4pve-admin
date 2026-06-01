# <span class="ee"></span> :material-monitor-dashboard: VM Performance <span class="scope" data-scope="per-cluster"></span>

Real-time per-disk I/O monitoring for running QEMU virtual machines — bytes read/written, operations and derived rates per second.

## Features

<div class="grid cards" markdown>

- :material-harddisk:{ .lg .middle } **Per-device I/O**

    ---

    Bytes read/written and read/write IOPS exposed per virtual disk device, not just per VM.

- :material-speedometer:{ .lg .middle } **Derived Rates**

    ---

    Read/Write bytes/sec and IOPS/sec computed from the delta between two consecutive samples.

- :material-server:{ .lg .middle } **Grouped View**

    ---

    Results grouped by Node and then by VM ID — drill straight to the noisy disk.

- :material-refresh:{ .lg .middle } **On-demand Refresh**

    ---

    Click refresh to take a new sample — the next reading recomputes per-second rates against the previous one.

</div>

## Why

Why a separate VM I/O view when PVE already shows VM activity?

<div class="why-grid" markdown>

<div markdown>
!!! tip "Per-disk granularity"
    PVE summarises VM I/O as a single number. This view splits it per virtual device (`scsi0`, `virtio1`, …) so you can tell which disk is the hot one.
</div>

<div markdown>
!!! success "IOPS, not just bytes"
    Both throughput (B/s) and operations (IOPS/s) are exposed — useful when latency, not bandwidth, is the bottleneck.
</div>

<div markdown>
!!! info "QEMU Monitor direct"
    Data comes straight from `info blockstats` on the QEMU Monitor — bypasses the RRD aggregation window, so spikes are visible.
</div>

<div markdown>
!!! warning "QEMU running only"
    The view lists running QEMU VMs. LXC containers and stopped VMs do not appear here — there is no `blockstats` source for them.
</div>

</div>

## Sections

- **Status** — grid of every running QEMU VM with its block devices: cumulative read/write bytes, read/write IOPS, and the per-second rates derived against the previous refresh. Grouped by Node → VM ID

!!! info "How rates are computed"
    Read/Write bytes/sec and IOPS/sec are computed as `(current - previous) / elapsed_seconds`. The first sample shows raw counters only; the per-second columns populate from the second refresh onwards.
