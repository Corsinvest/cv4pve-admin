# Changelog

All notable changes to cv4pve-admin will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

## [2.1.0-rc1] - 2026-06-01

### Community Edition

#### Added

- **Diagnostic**
  - PDF and Excel reports are now available in the free edition (used to be Enterprise-only). The PDF lists every issue colour-coded by severity and a separate section with the issues you have chosen to ignore. The Excel workbook opens on a cover sheet and the data sheet has the autofilter already on, so you can slice and pivot straight away.
  - **Compliance mapping**. Every check is tagged with the controls it covers across ISO/IEC 27001, EU NIS2, EU DORA and PCI DSS v4.0. The scan details now have a *Compliance* tab that groups findings by standard and control; the mapping is saved with the scan, so older reports stay consistent.
  - **Audit mode**: a new switch makes each passing check appear as a *Pass* result — handy when you have to prove that a control was actually verified, not only when it failed.
  - **Attach PDF / Excel** toggles in the notifier settings: choose what to send with scheduled scan emails.
  - New checks: ZFS pool detail, LVM-thin metadata, S.M.A.R.T. temperature and SSD wearout thresholds, PSI Pressure thresholds (CPU / I/O / Memory), snapshot age and backup recency (RPO).
  - Each issue links straight to the relevant Proxmox VE documentation page (QEMU guest agent, VirtIO drivers, end-of-life OS).
  - The *Download* button on the Scans page is a split-button: one click for the PDF (default), use the arrow to pick the Excel.

- **Updater**
  - PDF and Excel reports in the free edition (used to be a placeholder). The PDF lists every VM/CT and the status of its update scan; if some hosts failed the scan they get their own *Errors* page. The Excel workbook mirrors the same layout.
  - **Parallel scan** with a configurable *Max Parallel Requests* (1–50). A single host failing no longer stops the whole scan: the error is recorded against that host and the rest of the run continues.
  - Clicking *Scan* now starts the job immediately (the previous 5-second delay is gone).
  - Same PDF / Excel split-button on the Scans page.

- **System Report**
  - **Three output formats**: **Excel** (the existing one), **HTML** (a single browseable page with a table of contents and cross-section links) and **JSON** (machine-readable, with raw values and clean slugs).
  - **One zip file as result**, whatever the format: the zip contains the report itself plus the SVG network diagram, so you have everything in a single download.
  - The chosen format is **saved with the report** — opening an old report from the list reminds you how it was generated.
  - *Read-only* mode for the report dialog: click the id in the grid to inspect a past report's settings without risk of editing them.
  - The page is now called *Reports* in the side menu (was *Scans*).

- **AutoSnap**
  - **Max Parallel** option per job (1–10) so snapshots within the same run can execute in parallel.

- **AI Server**
  - New MCP tools: list/download/delete ISO images, list templates, delete backups, delete arbitrary storage content, read cluster defaults (migration, bandwidth, HA, console).

- **Metrics Exporter**
  - **Per-cluster configuration** (used to be global): each cluster has its own on/off, its own collectors and its own cache windows.
  - Master *Enabled* switch, plus per-exporter and per-collector switches with editable cache seconds.
  - *Fast / Standard / Full* preset buttons to apply common configurations in one click.
  - The Status page shows only the current cluster, with one panel per exporter (degrades to an info message when disabled).

- **Node Protect**
  - Folder backup runs now appear in *System → Tasks* alongside AutoSnap, System Report and Diagnostic, with a clickable deep link and per-node summary lines.
  - Updated default *Paths to backup*: dropped wildcard entries that didn't expand correctly when quoted, added `/var/spool/cron/crontabs` so per-user cron jobs are picked up.

- **Resources**
  - **Three new pages** in the side menu: **Networks**, **Disks** and **Partitions**, each with its own filters, grouping and card / list view switcher.
  - **Networks** has four tabs:
    - *Nodes* — per-node bridges and bonds
    - *Guests* — per-VM virtual NICs with model, MAC, bridge, firewall flag and IP addresses
    - *SDN* — virtual networks with Zone, Zone Type, Bridge, Tag, Alias, Nodes
    - *Diagram* — downloadable SVG topology view
  - **Disks** lists the physical disks across the cluster with disk **Kind** (HDD / SSD / NVMe), model, serial, health and S.M.A.R.T. summary, with a drill-down per disk.
  - **Partitions** shows the partition / mount table per node.
  - **Snapshots** has a new *Trends* tab next to the snapshot list, showing how snapshot size changes over time (visible when *Calculate snapshot size* is enabled on the cluster).
  - **Storages** reorganised into tabs: *Configuration*, *Storages*, *Usage* (with sub-tabs *By Storage* and *By VMs*).
  - **VMs** page renamed to **Guests** (it has always included LXC containers, not just QEMU VMs — the name now reflects the content).
  - **Card view** alongside the list view: every Guest / Node / Storage gets a card with status and key metrics; click to expand the card full-width. Toolbar with type / status / tag toggles, search and grouping.
  - Unified *Health Score* indicator: shows `N/A` when no data is available, with a tooltip explaining why (e.g. *VM stopped — score not available*).
  - Guest cards and lists show the operating system icon with a tooltip.

- **System**
  - The **Tasks** page has a search box in the toolbar that filters across Title, Cluster, Module, Phase, Created by and Last log.
  - The **Active Tasks** menu auto-clears the badge for failed / abandoned tasks once you have opened the panel. The *Active* filter is now explicit: running tasks + ended in the last 10 minutes + unacknowledged failures.
  - **New user activation flow**: when an admin creates a user from the UI the new user receives an activation email with a password-reset link, instead of being created without a password.
  - **User unlock** command added to the CLI, so you can reset a locked-out account without going through the UI.
  - Cluster names with spaces or special characters in the URL no longer break navigation to module pages.
  - **Maintenance: *Compact (full)*** — new button that reclaims disk space after big cleanups (audit logs, task history, old reports). The log shows the size of each module's data before and after, so you can see exactly how much was freed. The action is opt-in only, asks for explicit confirmation, and is not run by *Fix All* — tables are locked while it runs.
  - **Help bundled with the app** — the user documentation is now included in the container and served locally. The `?` menu opens it without needing internet access; a second entry **Documentation (online)** still points to the latest version on the project website.

- **Command palette**
  - The `ip:` filter now shows one IPv4 badge per VM (IPv6 and loopback addresses filtered out).

#### Changed

- **Diagnostic**
  - Settings screen redesigned with one accordion per area (Node, QEMU, LXC, Storage, Snapshot, Backup, CVE) and per-context thresholds clearly grouped.
  - CVE checks now look up Proxmox VE specific CVEs in NVD using a CPE filter — wider coverage than the previous Debian-only tracker (which has been removed from the settings).

- **Resources**
  - The list view has been redesigned alongside the new card view (filters, grouping and search work consistently across both).

- **Task Tracking**
  - The Active Tasks panel and menu refresh row-by-row instead of reloading the whole grid, so the UI no longer flickers when tasks update.

#### Fixed

- **Diagnostic**
  - Excel export no longer fails on scans where every issue is ignored.
  - PDF reports wrap long text inside every cell — resource id, sub-context and description — instead of running off the right edge of the page. This shows up most often when an issue description carries a long error message returned by the API.

- **System**
  - Cluster names with spaces no longer break navigation between module pages.
  - Validation messages inside edit dialogs are shown again (the form highlights the field with the error and a summary appears at the top).
  - Pages with many permission checks load fast, even on installations with many admin roles — they used to freeze for several seconds.
  - The "New Cluster" form no longer refuses the first submit because of a *PveName is required* error (the field is filled in automatically once the form is sent).
  - Release notes dialog shows only the current version, not earlier release candidates.
  - QEMU agent network cache refresh dropped from 30 to 15 minutes, so freshly attached NICs appear faster.
  - **Maintenance: *Reindex* and *Optimize* no longer touch other modules' data**. They used to run on the whole database once per installed module — so on an installation with sixteen modules each action ran sixteen times over everything. Now they run once per module, on that module's data only, and complete in a fraction of the time. *Reindex* also works again on databases whose name contains a hyphen (e.g. `cv4pve-admin`).

- **Notifier**
  - SMTP and WebHook editors refresh correctly when you pick a different notifier from the list (no more leftover bindings from the previous one).

- **Docker**
  - The container health check no longer marks the running container as unhealthy on every probe.

### Enterprise Edition

#### Added

- **Diagnostic — Compliance report**
  - The PDF gains one section per standard (ISO 27001, NIS2, DORA, PCI DSS). Every section starts on a new page with a short disclaimer, a summary table (Control / Title / Critical / Warning / Info / Ok / Status) and a per-control detail block listing the underlying findings.
  - The Excel workbook gains one *Compliance - &lt;Standard&gt;* sheet per standard, each with the same disclaimer, a *Summary* table and a *Details* table for filtering and pivoting.
  - In the scan details page the *Compliance* tab is a real grid grouped by Standard → Control, with the human-readable control title shown next to its id, and clickable links to the affected resources.
  - Colour coding on Gravity / Status cells is consistent between PDF and Excel.

- **System / Logs**
  - New native *Logs* page (replaces the Serilog UI integration) with server-side filtering by level, message and source.

- **System / Security**
  - When an admin creates a new user, a confirmation email is sent automatically — same flow as the Community self-registration. The new user receives an *activate your account* link instead of being stuck with no password.
  - Audit log detail dialog has a copy-to-clipboard button.

- **Portal**
  - Tenant user dialog has a new *Display Name* field; the previous *Admin* checkbox is now clearly labelled *Tenant Admin*. The *UserName* field only accepts email addresses, and is editable only when creating a new user.
  - Tenants without VMs show a friendly note instead of an empty table.

- **Node Protect / Git**
  - Git push runs appear in the task tracker with a clickable link to the Git page, and the task log lines now show remote URL, branch and outcome.

- **Updater**
  - The Enterprise PDF builds on top of the new free-edition PDF and still includes the Executive Summary at the top.

- **Workflow**
  - New built-in activities to query the cluster (replications, guest config, guests, RRD data) and pickers for HA groups, node names, storage names and VM ids.

#### Changed

- **Notifications**
  - Updated. Microsoft Teams is no longer supported; ntfy.sh users may need to update their URL.

#### Fixed

- **Diagnostic / Updater**
  - PDF tables wrap long values correctly (shared fix with the Community edition).

- **Workflow**
  - The workflow editor no longer fails to start in tenant mode (the live-update channel authenticates correctly).

## [2.0.0] - 2026-03-09

> 🎉 After over a year of complete rewrite, **cv4pve-admin 2.0.0** is here.
> Rebuilt from scratch on .NET 10, Blazor, and Radzen — modern architecture,
> modern stack, modern UX. Thank you to all testers and contributors. 🙏

#### Platform

- Multi-cluster management from a single interface
- Health Score for nodes, VMs, and storages
- Quick command search filtered by user permissions
- Fine-grained user roles and permissions per module
- Audit logging for security-relevant events
- Localization with per-user language/culture selection
- Bookmarkable URLs for every cluster and module
- Real-time progress tracking for running operations
- Dark/light theme, keyboard shortcuts
- Docker, native, and .NET Aspire deployment support
- Notification system with email and WebHook support *(extended channels available in Enterprise Edition)*

### Community Edition

#### Modules

- **AutoSnap** — automated snapshot scheduling with retention policies, hooks, and timeline view
- **Backup Analytics** — backup trend analysis with size, speed, and duration charts
- **Replication Analytics** — replication job monitoring and failure tracking
- **Diagnostic** — cluster health diagnostics with issue scanning and PDF export
- **Dashboard** — customizable widget-based dashboard (gauges, charts, sparklines, heatmaps)
- **Node Protect** — node configuration backup with multiple storage backends
- **Metrics Exporter** — export cluster metrics to external monitoring systems
- **AI Server** — connect Proxmox VE to AI assistants (Claude, ChatGPT, Cursor…) via Model Context Protocol
- **Bots** — Telegram bot for remote cluster management
- **UPS Monitor** — UPS status monitoring via NUT protocol
- **System Report** — system information and configuration report generation
- **Updater** — application self-update management
- **Resources** — cluster resource overview with Health Score, usage charts, and bookmarkable URLs

### Enterprise Edition

#### Additional Modules

- **Portal** — self-service portal for end-user VM management
- **Workflow** — visual workflow automation powered by Elsa
- **VM Performance** — advanced VM performance analytics and trending
- **DRS** — dynamic resource scheduling with HA groups and migration history
- **AI Server Enterprise** — AI Query Engine: ask questions about your infrastructure in natural language
- **System Enterprise** — advanced user management with roles, AppTokens (machine-to-machine API authentication with granular permissions), full audit log, SSO integration, and appearance customization
- **Diagnostic Enterprise** — extended diagnostics with enterprise-specific checks
- **Node Protect Enterprise** — additional storage providers for node configuration backup
- **Notifier Enterprise** — extended channels: Apprise, custom webhooks
- **Profile Enterprise** — extended user profile and SSO integration

## [2.0.0-rc6] - 2026-03-04

### Community Edition

#### Added

- **AI Server / MCP** — refactored MCP infrastructure with ToolHelper, McpBridge, ApiAccess/Bridge tabs
- **AI Server / MCP** — upgrade McpBridge to official MCP SDK 1.0.0; log MCP requests to audit log
- **Automatic API token wizard** — guided creation of API tokens during cluster registration
- **PvePermissions model** — cluster permission caching
- **Health Score** — for nodes, VMs, and storages; added to Resources column picker and widget icons
- **Sparklines** — CPU/RAM history sparklines in VM and Node summary components
- **Cluster name in URL** — multi-tab isolation and shareable deep links (`/module/{cluster}/{slug}`)
- **Query param deep-linking** — URL navigation in Resources, VM/Node managers, and search system
- **Localization system** — JSON-based localization with culture support
- **Culture preference** — language/culture selection in user profile
- **CopyValueDialog / CredentialDialog** — shared copy and credential display components
- **OpenCopyValueAsync** — dialog helper for token/value display
- **Resizable side dialog** — drag-to-resize side panel; EnumDropDown component
- **SshRequiredGate** — gate component in NodeProtect/Updater for SSH requirement enforcement
- **Disable console/SSH** for non-PAM or API Token users
- **Default password notification** — alert when default admin password is still in use
- **Icons** — added to Diagnostic IssuesDialog fields and HealthScore widget
- **Dynamic Documentation link** — help menu link adapts to current module
- **"Who is using"** — HelpMenu entry with cluster info pre-fill

#### Changed

- Move ClusterSelector to Layout namespace; add UrlHelper
- Move SettingSection/SettingsAccordion from Module.System to Core
- Standardize audit logging format
- Unify Job/ActionHelper scan signature across analytics modules
- Core misc improvements — BuildInfo.IsInContainer, SyncRolesAsync, EditDialog cleanup
- Extract default admin credentials into ApplicationHelper constants
- Update default dashboard configuration
- Simplify AI Server VmTools; add tags to minimal VM output
- Remove redundant title/aria-label from buttons with visible Text
- Upgrade Radzen to 9.0.8

#### Fixed

- Prevent concurrent RefreshDataAsync calls with SemaphoreSlim
- Expand resource rows after data load instead of OnAfterRenderAsync
- PasswordField Start/End slots reorder; use Variant.Text for toggle
- Various minor UI and logic fixes

### Enterprise Edition

#### Added

- **DRS** — unified Dashboard with Resource Distribution (CPU/RAM barchart + RRD sparklines), HA Groups support, full run history with migrations detail
- **AI Server** — new MCP tools: `ListBackups`, `ListStorageContent`, `ListTasks`; PSI Pressure metrics in RRD data
- **AppTokens** — authentication with bulk permission operations; AppTokenPermissionsGrid, PermissionsSummaryDialog
- **PermissionsEditor** — AppTokens UI, built-in role protection, PermissionsSummaryDialog service
- **AIServer EE permissions** — extended MCP tool permissions model

#### Changed

- Audit log — copy button in audit log details
- Adapt EE to URL-based cluster routing (cluster name in URL)

## [2.0.0-rc5] - 2026-02-28

### Added

- **AI Server** — McpBridge build support; PVEAdmin role docs and AI Server output formats
- **Connecting AI Clients** — MCP Bridge section in AI Server documentation

### Changed

- Update NuGet packages

### Fixed

- CSS assets Docker build alignment

## [2.0.0-rc4] - 2026-02-19

### Fixed

- CSS assets returning 500 error in production (Release build)
- Docker build aligned to production configuration

## [2.0.0-rc3] - 2026-02-19

### Added

- **RangeSelector component** — date/time range selection with drag handles
- **"Add cluster" button** — pulsing button in header when no clusters are configured
- **Memo widget** — content editing moved to settings panel
- **Subscription page** *(EE)* — alert shown when running in testing/pre-release mode

### Changed

- Dashboard widget title bar hidden when title is empty
- Dashboard clone now copies widget size
- ResourceUsageGaugeStacked default size adjusted

### Fixed

- Cluster deletion now refreshes header and navigation immediately
- Various Docker startup and CSS asset fixes

## [2.0.0-rc2] - 2026-02-18

### Added

- **WebHook notifier module** — send notifications to any HTTP endpoint on Proxmox VE events
- **MarkdownEditor component** — Write/Preview tabs for markdown fields across the UI
- **ResourceUsageGaugeStacked widget** — concentric arc gauges for CPU, memory and storage usage
- **Help menu** — separate items for Report a Bug, Request a Feature and Provide Feedback, with bug report pre-filled with version, cluster, Proxmox VE version, browser and platform
- **Help menu** — red dot badge on help icon and "Update available" notification when a new release is available
- **Icons** added to form fields across the UI for better visual clarity
- **HTTPS reverse proxy guide** in documentation

### Changed

- AutoSnap webhooks refactored to use ExtendedData for cleaner configuration
- Dashboard reset logic improved, save icon added
- Send test email button placed inline with the email field
- Telegram bot info dialog replaced with link to documentation

### Fixed

- Installer interactive prompts fixed
- `appsettings.extra.json` pre-created on Windows to prevent Docker creating it as a directory

## [2.0.0-rc1] - 2026-02-12

### Initial Release

This is the first public release of **cv4pve-admin v2** — a complete rewrite of the original project built on .NET 9 and Blazor.

Starting from this release, all notable changes will be tracked in this file.

#### Modules (Community Edition)

- **AutoSnap** — Automated snapshot scheduling with hooks, history, and timeline view
- **Backup Analytics** — Backup trend analysis with size, speed, and duration charts
- **Replication Analytics** — Replication job monitoring and failure tracking
- **Diagnostic** — Cluster health diagnostics with issue scanning and PDF export
- **Dashboard** — Customizable widget-based dashboard with multi-cluster support
- **Node Protect** — Node configuration backup with pluggable storage providers
- **Metrics Exporter** — Export cluster metrics to external monitoring systems
- **Notifier** — Notification system with SMTP support
- **AI Server** — Model Context Protocol (MCP) server for AI assistant integration
- **Bots** — Telegram bot integration for remote cluster management
- **UPS Monitor** — UPS status monitoring via NUT protocol
- **System Report** — System information and configuration report generation
- **Updater** — Application self-update management
- **Resources** — Cluster resource usage overview

#### Modules (Enterprise Edition)

- **Portal** — Self-service portal for VM management by end users
- **Workflow** — Visual workflow automation for Proxmox VE operations (powered by Elsa)
- **VM Performance** — Advanced VM performance analytics and trending
- **Diagnostic Enterprise** — Extended diagnostics with enterprise checks
- **Node Protect Enterprise** — Extended node protection with additional providers
- **Notifier Enterprise** — Extended notification channels (Apprise, webhooks, etc.)
- **AI Server Enterprise** — Extended MCP tools for enterprise operations
- **System Enterprise** — Extended system management and configuration
- **Profile Enterprise** — Extended user profile and SSO integration

#### Core Features

- Multi-cluster management from a single interface
- Command Palette for quick navigation and actions
- Role-based access control with granular module permissions
- Audit logging for security-relevant events
- Dark/light theme with Fluent design
- Keyboard shortcuts
- Help menu with release notes, documentation links, and feedback
- Docker, native, and .NET Aspire (AppHost) deployment support
