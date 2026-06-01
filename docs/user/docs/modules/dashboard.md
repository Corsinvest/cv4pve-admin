# :material-view-dashboard: Dashboard <span class="scope" data-scope="all-clusters"></span>

Customizable dashboards with widgets and metrics for centralised Proxmox VE infrastructure monitoring. Each dashboard is personal, fully customisable and shareable.

## Features

<div class="grid cards" markdown>

- :material-view-dashboard:{ .lg .middle } **Multiple Dashboards**

    ---

    Create, clone, rename and delete as many dashboards as needed. Each dashboard is personal and independent.

- :material-widgets:{ .lg .middle } **Widget Library**

    ---

    Add widgets from any installed module (Resources, AutoSnap, Backup Analytics, Diagnostics, and more). Each widget can be resized and repositioned freely on the grid.

- :material-server-network:{ .lg .middle } **Multi-Cluster Filter**

    ---

    Enable cluster selection per dashboard to filter widget data by one or more clusters.

- :material-refresh:{ .lg .middle } **Auto Refresh**

    ---

    Configure an automatic refresh interval (in seconds) to keep all widget data up to date without manual interaction.

- :material-monitor:{ .lg .middle } **Screen Resolution Preview**

    ---

    Preview how the dashboard looks at different screen resolutions (Full HD, HD+, WXGA+, and more) while editing.

- :material-export:{ .lg .middle } **Import / Export**

    ---

    Export a dashboard to JSON and import it on another instance or share it with other users.

</div>

## Why

Why a custom dashboard when PVE has its own summary?

<div class="why-grid" markdown>

<div markdown>
!!! tip "What you care about, not what's default"
    Compose the view: backup status + diagnostic alerts + cluster gauges + a Memo with the on-call rota. PVE shows the same thing to everyone.
</div>

<div markdown>
!!! success "Personal, not global"
    Each user has their own dashboards — change yours without affecting colleagues, and theirs without seeing yours.
</div>

<div markdown>
!!! info "Aggregates from every module"
    Widgets surface data from Resources, AutoSnap, Backup Analytics, Replication Analytics, Diagnostics, Node Protect — one screen, every signal.
</div>

<div markdown>
!!! warning "Auto-refresh + export"
    Set the refresh interval once; the dashboard stays current on a wall display. Export to JSON to share a standard "ops board" across the team.
</div>

</div>

## Edit Mode

Click the **Edit** button to enter edit mode. In edit mode you can:

- **Add widgets** — select a widget from the module widget menu and it is placed automatically in the first free position on the grid.
- **Move and resize** — drag widgets to reposition them; drag the resize handle to change size.
- **Configure** — open the widget settings dialog to customize title, CSS classes, and widget-specific options.
- **Clone** — duplicate a widget with the same settings.
- **Remove** — delete a widget from the dashboard.
- **Save / Cancel** — save persists all changes to the database; cancel discards unsaved changes (with confirmation if changes exist).

## Widgets

The Dashboard ships two generic widgets always available; every other module contributes its own widgets as soon as it is enabled.

??? note widgets "Show all available widgets"

    | Module | Widget | Description |
    |--------|--------|-------------|
    | **Dashboard** (built-in) | Web Content | Displays a link (icon + label), an iframe, or raw HTML content |
    | **Dashboard** (built-in) | Memo | Markdown note pad — write formatted notes inside the widget |
    | Resources | Resources Status | Shows the status of cluster resources |
    | Resources | Resources Usage | Shows resource usage metrics |
    | Resources | Cluster Usage Gauge | Concentric arc gauge for cluster CPU/RAM/storage usage |
    | AutoSnap | Status | AutoSnap job status overview |
    | AutoSnap | Size | Snapshot size over time chart |
    | AutoSnap | Check | Failed snapshots alert |
    | AutoSnap | Info | Snapshot statistics and insights |
    | Backup Analytics | Status | Backup job status overview |
    | Backup Analytics | Size | Backup size chart |
    | Backup Analytics | Check | Failed backups alert |
    | Backup Analytics | Info | Backup statistics |
    | Diagnostics | Status | Diagnostic check status |
    | Diagnostics | Check | Diagnostic issues list |
    | Replication Analytics | Status | Replication status overview |
    | Replication Analytics | Size | Replication size chart |
    | Replication Analytics | Check | Replication check alerts |
    | Replication Analytics | Info | Replication analytics overview |
    | Node Protect | Folder Size | Node Protect backup folder size |
    | Node Protect | Git Size | <span class="ee"></span> Node Protect Git repository size |

## Default Dashboard

On first access, a default dashboard is created automatically from a built-in template. You can reset to defaults at any time using the **Create Default** action.
