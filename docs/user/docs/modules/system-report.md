# :material-file-document: System Report <span class="scope" data-scope="per-cluster"></span>

Snapshots the state of a Proxmox VE cluster (configuration, resources, audit data) into a single downloadable file. Useful for compliance audits, customer hand-over, change tracking and capacity planning. Built on the [`cv4pve-report`](https://github.com/Corsinvest/cv4pve-report) library.

## Features

<div class="grid cards" markdown>

-   :material-file-export:{ .lg .middle } **Multiple Formats**

    ---

    Export as **Xlsx** (single workbook with one sheet per section), **Html** (browsable static site) or **Json** (multi-file, one per section). Html and Json formats are packaged as a single `.zip`.

-   :material-tune-variant:{ .lg .middle } **Presets & Fine-grained Settings**

    ---

    Pick a preset — **Fast** (structure only, no heavy data), **Standard** (default), **Full** (RRD on week timeframe, syslog, firewall log, S.M.A.R.T.) — or toggle every individual section yourself.

-   :material-toggle-switch-outline:{ .lg .middle } **What's Included**

    ---

    Cluster (overview, audit log, tasks) · Nodes (detail, APT updates, replication, RRD, syslog, S.M.A.R.T.) · Guests (snapshots, disks, partitions, QEMU agent, RRD) · Storage (content, backups, RRD) · Firewall (rules, aliases, ipsets, log).

-   :material-filter-variant:{ .lg .middle } **Resource Filters**

    ---

    Limit by node names (`@all`, comma-separated, wildcards) and by guest IDs.

-   :material-play-circle:{ .lg .middle } **On-demand Generation**

    ---

    Generate reports manually from the Reports page — each run is persisted with the format and settings you chose. The history grid lets you download any past report or re-run it.

-   :material-bolt:{ .lg .middle } **Parallel Fetching**

    ---

    Configurable `MaxParallelRequests` (default 5) speeds up data collection on large clusters; set to 1 to fall back to sequential mode.

</div>

## Why

Why automate reports when PVE already shows everything in its UI?

<div class="why-grid" markdown>

<div markdown>
!!! tip "One file you can hand off"
    Auditors, customers, capacity planners want a self-contained file. Xlsx / Html / Json — pick the one that fits the receiver.
</div>

<div markdown>
!!! success "Snapshot a point in time"
    Reports are persisted with the settings you chose — go back six months and see exactly what the cluster looked like then.
</div>

<div markdown>
!!! info "Heavy data on demand"
    SMART, syslog, firewall log, week-long RRD — opt in only when you need them. Fast preset stays cheap and short.
</div>

<div markdown>
!!! warning "Compliance ready"
    PVE shows live state. A System Report is dated, archivable, and answers "what was running, where, when?" — what auditors actually ask.
</div>

</div>

## Sections

- **Reports** — browse the history of generated reports, download a previous report, trigger a new one with custom settings or one of the presets (Fast / Standard / Full)

## Settings

??? note settings "Show all settings"

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **Enabled** | off | Master on/off switch for the module on the cluster |

    !!! info "Per-report settings"
        Format (Xlsx / Html / Json), preset (Fast / Standard / Full) and every individual section toggle are configured per-report when you trigger generation — not at module level.
