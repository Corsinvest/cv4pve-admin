# :material-update: Update Manager <span class="scope" data-scope="per-cluster"></span>

Scans VM/CT in Proxmox VE to identify available updates including security patches. Read-only — provides visibility into pending updates without making changes.

## Features

<div class="grid cards" markdown>

-   :material-security:{ .lg .middle } **Security Patches**

    ---

    Identify critical security patches across VMs and containers.

-   :material-package-variant:{ .lg .middle } **Update Inventory**

    ---

    View available updates by system, package, and severity level.

--8<-- "_includes/feature-cron.md"

-   :material-shield-search:{ .lg .middle } **Read-Only Scanning**

    ---

    Check for updates without making system changes.

--8<-- "_includes/feature-export-pdf-excel.md"

--8<-- "_includes/feature-parallel-scan.md"

--8<-- "_includes/feature-notifier.md"

</div>

## Why

Why centralise update checks when `apt list --upgradable` already exists?

<div class="why-grid" markdown>

<div markdown>
!!! tip "One view, every VM"
    See pending updates across all running VMs and CTs of the cluster on a single page — no per-host SSH session, no script-on-script.
</div>

<div markdown>
!!! success "Security signal isolated"
    Each row carries Normal / Security / Reboot-required flags so the security backlog never gets buried in the rest.
</div>

<div markdown>
!!! info "Read-only by design"
    The module never patches anything. Safe to schedule and forget — you stay in control of when and how to apply updates.
</div>

<div markdown>
!!! warning "One broken VM doesn't break the run"
    SSH timeout, missing agent or parse error are isolated to that single item — the rest of the scan completes and the failure surfaces in the report.
</div>

</div>

## Sections

- **Scans** — live grid of every running VM/CT with its current scan status (Ok / InError / Cancelled), available updates (Normal / Security), reboot required flag. Trigger a new scan, download the report as PDF or Excel

## Settings

??? note settings "Show all settings"

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **Enabled schedulation** | off | Master on/off switch for the scheduled scan |
    | **Cron Expression** | `0 */12 * * *` [:material-open-in-new:](https://crontab.guru/#0_*/12_*_*_*){target=_blank title="Open on crontab.guru"} | When the scheduled scan runs |
    | **Max Parallel Requests** | 5 | Max concurrent VM/CT scans (1 = sequential, range 1-50) |
    | **Script Windows Search Update** | bundled PowerShell | Script run inside Windows guests to detect updates — restorable to default with the refresh button |
    | **Script Linux Search Update** | bundled bash | Script run inside Linux guests / LXC to detect updates — restorable to default with the refresh button |
    | **Notifier Configurations** | – | List of Notifier configurations to deliver the report to |

--8<-- "_includes/requirements-ssh.md"
