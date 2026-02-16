# AutoSnap

Automated snapshot scheduling and management for Proxmox VE virtual machines.

## Overview

Automated snapshot creation, rotation, and retention management for Proxmox VE infrastructure.

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

-   :material-webhook:{ .lg .middle } **Web API Hook** <span class="ee"></span>

    ---

    Trigger HTTP webhooks on snapshot phase events (before/after create, before/after delete). Configure per-job HTTP endpoints with custom headers and body templates.

</div>
