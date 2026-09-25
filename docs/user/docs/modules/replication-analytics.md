# :material-sync: Replication Analytics <span class="scope" data-scope="per-cluster"></span>

Monitors Proxmox VE replication jobs across the cluster: status, trends, error patterns.

## Features

<div class="grid cards" markdown>

- :material-shield-check:{ .lg .middle } **Replication Status**

    ---

    Monitor VM and CT replication job execution and success rates.

- :material-chart-line:{ .lg .middle } **Performance Metrics**

    ---

    Track replication size, duration, and failure patterns.

- :material-monitor-eye:{ .lg .middle } **On-demand or scheduled scan**

    ---

    Collect replication run history on demand or on a schedule using a cron expression.

- :material-link-variant:{ .lg .middle } **Proxmox VE Integration**

    ---

    Works with Proxmox VE replication jobs via API.

- :material-timeline-clock:{ .lg .middle } **Historical Data**

    ---

    View replication trends over time for optimization.

- :material-tools:{ .lg .middle } **Failure Logs**

    ---

    Access detailed logs and error information for failed replication jobs.

</div>

## Why

Why a dedicated view when PVE has its Replication tab?

<div class="why-grid" markdown>

<div markdown>
!!! tip "Cluster-wide, not per-node"
    PVE's Replication tab is per-node. This view rolls up every replication of the cluster on one page.
</div>

<div markdown>
!!! success "Trends, not just current state"
    Charts over time show size and duration drift — info that PVE doesn't keep anywhere.
</div>

<div markdown>
!!! info "Spot the silent failure"
    A job that started failing last week without anyone noticing — the run history keeps every failed run with its status and error.
</div>

<div markdown>
!!! warning "Failure context, not just status"
    Failed runs come with the underlying error message — find out why without SSH-ing into the source node.
</div>

</div>

## Sections

- **Replications** — history of collected replication runs with start/end, duration, size, status, source/target node, error and log
- **Scheduled** — configured replication jobs with schedule, rate limit, last sync and next sync
- **Trends** — charts of replication size and duration over time

## Settings

??? note settings "Show all settings"

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **Enabled schedulation** | off | Master on/off switch for the periodic data collection |
    | **Cron Expression** | `*/5 * * * *` [:material-open-in-new:](https://crontab.guru/#*/5_*_*_*_*){target=_blank title="Open on crontab.guru"} | When the analytics refresh runs (default: every 5 minutes) |
    | **Max Days Logs** | 30 | How many days of replication logs to retain for analysis |
