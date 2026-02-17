# Dashboard

Customizable dashboards with widgets and metrics for centralized Proxmox VE infrastructure monitoring.

## Overview

Create multiple personal dashboards by combining widgets from different modules. Each dashboard is per-user and fully customizable: choose which clusters to monitor, set an auto-refresh interval, arrange widgets freely on a 24×24 grid.

<div class="grid cards" markdown>

-   :material-view-dashboard:{ .lg .middle } **Multiple Dashboards**

    ---

    Create, clone, rename and delete as many dashboards as needed. Each dashboard is personal and independent.

-   :material-widgets:{ .lg .middle } **Widget Library**

    ---

    Add widgets from any installed module (Resources, AutoSnap, Backup Analytics, Diagnostics, and more). Each widget can be resized and repositioned freely on the grid.

-   :material-server-network:{ .lg .middle } **Multi-Cluster Filter**

    ---

    Enable cluster selection per dashboard to filter widget data by one or more clusters.

-   :material-refresh:{ .lg .middle } **Auto Refresh**

    ---

    Configure an automatic refresh interval (in seconds) to keep all widget data up to date without manual interaction.

-   :material-monitor:{ .lg .middle } **Screen Resolution Preview**

    ---

    Preview how the dashboard looks at different screen resolutions (Full HD, HD+, WXGA+, and more) while editing.

-   :material-export:{ .lg .middle } **Import / Export**

    ---

    Export a dashboard to JSON and import it on another instance or share it with other users.

</div>

## Edit Mode

Click the **Edit** button to enter edit mode. In edit mode you can:

- **Add widgets** — select a widget from the module widget menu and it is placed automatically in the first free position on the grid.
- **Move and resize** — drag widgets to reposition them; drag the resize handle to change size.
- **Configure** — open the widget settings dialog to customize title, CSS classes, and widget-specific options.
- **Clone** — duplicate a widget with the same settings.
- **Remove** — delete a widget from the dashboard.
- **Save / Cancel** — save persists all changes to the database; cancel discards unsaved changes (with confirmation if changes exist).

## Built-in Widgets

The Dashboard module provides two generic widgets available in all editions:

| Widget | Description |
|--------|-------------|
| **Web Content** | Displays a link (with icon and label), an iframe, or raw HTML content. Configurable URL, icon, text, and HTML. |
| **Memo** | A Markdown note pad. Write formatted notes directly inside the widget. |

## Widgets from Other Modules

When other modules are installed, their widgets become available in the Dashboard widget library:

| Module | Widget | Description |
|--------|--------|-------------|
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
| Node Protect | Folder Size | NodeProtect backup folder size |
| Node Protect | Git Size | NodeProtect Git repository size <span class="ee"></span> |

## Default Dashboard

On first access, a default dashboard is created automatically from a built-in template. You can reset to defaults at any time using the **Create Default** action.
