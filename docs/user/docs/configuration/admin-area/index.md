# :material-shield-crown: Admin Area

System administration, security, monitoring and Proxmox VE cluster configuration — all under one menu.

## Features

<div class="grid cards" markdown>

- :material-dns:{ .lg .middle } **[Proxmox VE Clusters](clusters.md)**

    ---

    Connect and manage clusters: Web API credentials, SSH credentials, node ordering.

- <span class="ee"></span> :material-security:{ .lg .middle } **[Security & Access Control](security.md)**

    ---

    Users, roles, permissions, App Tokens, active sessions and audit logs.

- :material-monitor-eye:{ .lg .middle } **[Monitoring](monitoring.md)**

    ---

    System information, application logs <span class="ee"></span> and background jobs (Hangfire dashboard).

- :material-wrench:{ .lg .middle } **[Maintenance](maintenance.md)**

    ---

    Database operations (reindex, optimize, compact), cleanup retention, cache management, connectivity tests.

- :material-cog:{ .lg .middle } **[General Settings](general-settings.md)**

    ---

    Global app configuration: name, theme, SMTP, appearance <span class="ee"></span>, release channel.

- :material-bell:{ .lg .middle } **[Notification Hub](notifier.md)**

    ---

    Notification channels — SMTP and WebHook in CE, 140+ services via Apprise <span class="ee"></span>.

</div>

## Why

<div class="why-grid" markdown>

<div markdown>
!!! tip "One control plane for all clusters"
    Connect every Proxmox VE cluster you manage and operate them from a single UI — no more juggling browser tabs.
</div>

<div markdown>
!!! success "Fine-grained access control"
    <span class="ee"></span> Roles + permissions let you give the team read-only access, ops snapshot rights, or full admin — per module, per cluster.
</div>

<div markdown>
!!! info "Audit trail by default"
    <span class="ee"></span> Every administrative action is recorded — who changed what, when, on which cluster. Useful for compliance and post-incident review.
</div>

<div markdown>
!!! note "Built-in housekeeping"
    Maintenance routines are one click away: vacuum, reindex, log retention, cache flush — no `psql` shell required.
</div>

</div>

## Access

Open the Admin Area from the main menu or from the profile dropdown in the top navigation bar.
