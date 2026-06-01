# :material-backup-restore: Backup Analytics <span class="scope" data-scope="per-cluster"></span>

Track and analyze backup job performance across Proxmox VE clusters. Surfaces success rates, duration patterns, storage consumption and — crucially — what is **not** being backed up.

## Features

<div class="grid cards" markdown>

-   :material-chart-bar:{ .lg .middle } **Job Status**

    ---

    Monitor backup job execution and completion status across cluster nodes.

-   :material-speedometer:{ .lg .middle } **Performance Metrics**

    ---

    Track backup duration, throughput, and resource usage patterns.

-   :material-shield-alert:{ .lg .middle } **RPO Blind Spots**

    ---

    Dedicated views for guests and disks **not** covered by any backup job — find the gaps before they bite you.

-   :material-history:{ .lg .middle } **Historical Data**

    ---

    View backup trends and patterns over time for capacity planning.

-   :material-file-search:{ .lg .middle } **Failure Logs**

    ---

    Access detailed logs and error information for failed backup jobs.

-   :material-database-cog:{ .lg .middle } **Storage Metrics**

    ---

    Monitor backup storage consumption and growth trends.

</div>

## Why

Why analytics when PVE already shows backup jobs?

<div class="why-grid" markdown>

<div markdown>
!!! tip "What you're NOT backing up"
    The Unprotected Guests / Disks views surface VMs and disks that no job covers — the real reason silent data loss happens.
</div>

<div markdown>
!!! success "One view per cluster"
    Backups, schedule, timeline and trends roll up across nodes and storages — no per-node UI grepping.
</div>

<div markdown>
!!! info "Trends, not snapshots"
    Charts over time tell you whether duration is creeping up, storage is filling, or a job started failing weeks ago without anybody noticing.
</div>

<div markdown>
!!! warning "Failure context, not just rows"
    Failed runs link straight to logs and error messages — find out why one VM keeps failing without SSH-ing into the node.
</div>

</div>

## Sections

- **Backups** — per-VM backup history with date, size, duration, status and storage
- **Scheduled** — configured backup jobs with schedule, target storage and selection
- **Unprotected Guests** — VMs/CTs not covered by any backup job — your RPO blind spots
- **Unprotected Disks** — disks excluded from backup (e.g. `backup=0`) on otherwise protected guests
- **Backup in line** — timeline view of backup runs across nodes/storages
- **Trends** — charts of backup size, duration and success rate over time

## Settings

??? note settings "Show all settings"

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **Enabled schedulation** | off | Master on/off switch for the periodic data collection |
    | **Cron Expression** | `0 */1 * * *` [:material-open-in-new:](https://crontab.guru/#0_*/1_*_*_*){target=_blank title="Open on crontab.guru"} | When the analytics refresh runs (default: hourly) |
    | **Max Days Logs** | 30 | How many days of backup logs to retain for analysis |
