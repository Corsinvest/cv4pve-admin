# :material-camera: AutoSnap <span class="scope" data-scope="per-cluster"></span>

Automated snapshot scheduling and management for Proxmox VE virtual machines.

## Features

<div class="grid cards" markdown>

-   :material-auto-fix:{ .lg .middle } **Scheduled Snapshots**

    ---

    Create snapshots automatically based on cron schedule with retention policies.

-   :material-clock-check:{ .lg .middle } **Retention Policies**

    ---

    Define how many snapshots to keep per VM, with automatic cleanup of expired ones.

-   :material-briefcase-check:{ .lg .middle } **Job Management**

    ---

    Configure snapshot jobs with cron schedule, VM selection, execution history and status monitoring.

-   :material-database-check:{ .lg .middle } **Multi-VM Support**

    ---

    Apply a single job to multiple VMs and containers using filters or explicit selection.

-   :material-swap-horizontal:{ .lg .middle } **Node-independent**

    ---

    Jobs target VM IDs / tags / groups — never a specific node. When a VM migrates (manual or HA failover), the next scheduled snapshot just runs on its new home with no reconfiguration.

-   <span class="ee"></span> :material-webhook:{ .lg .middle } **Web API Hook**

    ---

    Trigger HTTP webhooks on snapshot phase events (before/after create, before/after delete). Configure per-job HTTP endpoints with custom headers and body templates.

    [Learn more](autosnap-webhook.md)

</div>

## Why

Proxmox can already take snapshots — why use a module for it?

<div class="why-grid" markdown>

<div markdown>
!!! tip "Snapshots actually happen"
    On a schedule **you** choose. No human has to remember before the next risky operation.
</div>

<div markdown>
!!! success "Manage 50 VMs as one set"
    A single job targets a group via tags, wildcards or explicit list — no copy-paste per VM.
</div>

<div markdown>
!!! tip "Different schedules per workload"
    Hourly for the database, nightly for dev VMs, weekly for templates — each job runs on its own cron, side by side on the same cluster.
</div>

<div markdown>
!!! info "Follows the VM, not the node"
    Jobs target VM IDs / tags. When a VM migrates to another node, the next snapshot just runs there — no reconfiguration needed.
</div>

<div markdown>
!!! info "Retention without thinking"
    Set `Keep=7` once; the module deletes the 8th snapshot for you. Plain Proxmox keeps them forever until someone notices.
</div>

<div markdown>
!!! warning "Storage-safe by default"
    `MaxPercentageStorage` skips the snapshot when the target pool is too full — your storage never fills because of automation.
</div>

<div markdown>
!!! note "Auditable history"
    Every run logged in the Jobs grid, Time line and Errors tab — you know what was snapshotted, when, and what failed.
</div>

</div>

## Sections

- **Jobs** — manage AutoSnap jobs: target VMs, schedule, retention, name template, optional Web API Hook
- **Time line** — visual timeline of snapshots created across jobs and VMs
- **Errors** — history of failed snapshot operations with the underlying error message
- **Status** — current snapshot inventory across the cluster with per-VM counts and last successful run

## Settings

??? note settings "Show all settings"

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **Enabled** | off | Master on/off switch for the cluster |
    | **Keep History** | 10 | Maximum number of past run results to keep per job |
    | **Notify** | None | When to send notifier events: never, on errors, always |
    | **Search Mode** | Managed | How snapshots created by AutoSnap are identified (`Managed` = recognised by name pattern only) |
    | **Timestamp Format** | engine default | Format applied to the `{timestamp}` placeholder in snapshot names |
    | **On Remove Job Remove Snapshots** | on | Delete the snapshots created by a job when the job itself is deleted |
    | **Max Percentage Storage** | 95 | Skip snapshot creation if the target storage is more full than this percentage — prevents filling the pool |
    | **Notifier Configurations** | – | List of Notifier configurations to deliver events to |
