# Changelog

All notable changes to cv4pve-admin will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Community Edition

#### Added

- **Check for updates**: a button in the Help menu looks for a new version right away, instead of waiting for the periodic check (up to 12 hours).

- **Diagnostic — profiles**: *Fast*, *Standard* and *Full* buttons in the settings, as in Report. *Fast* skips the slowest reads (backup content, snapshots, LVM-thin metadata) for a quick scan of large clusters; *Full* turns on every optional check (S.M.A.R.T., ZFS details, CVE lookup, OK results) for audits.

- **Diagnostic — IOWait threshold**: a node spending too much CPU time waiting for disks is now reported, with its own threshold (Warning 10%, Critical 25%). Before, the check used the CPU thresholds and never fired.

- **Diagnostic — new network checks**: a guest using a VLAN its bridge does not let out of the node, and a guest using a bridge missing on another node (migration or HA would fail).

- **Diagnostic — PDF cover page**: the PDF report opens with a cover page, as the Excel file does: report information and a table of contents with the page number of each section, clickable to jump there. The sections also appear in the bookmarks panel of the PDF viewer.

- **WebHook — custom headers**: HTTP headers can be added to a webhook notification (and to AutoSnap hooks *(EE)*), with placeholders in their values. The `%severity%` placeholder is now listed in the editor.

#### Changed

- **Help menu**: the version, edition and update check share a single line at the top.

- **Diagnostic**: resource links in the results (and in the Compliance view *(EE)*) open in a new tab, so the report stays open.

- **Container IP addresses**: running containers show the address they have right now, also when it comes from DHCP (`10.0.0.5/24 (dhcp)` instead of just `dhcp`), in the Resources network list, in the network diagram and in the IP search. Stopped containers and older Proxmox VE releases keep showing the configured value.

- **Resources network list**: new *Trunks* column for guest NICs.

- **Report**:
  - Containers show the IP they actually have, like in Resources.
  - VM trunk ports appear in the network sheet and in the diagram; Cluster Access reports the disabled two-factor entries.
  - Rows always come in the same order, with numbers sorted as numbers (`105` before `1000`), so two reports of an unchanged cluster differ only in live values.

- **WebHook — placeholders**: values are escaped for where they are placed: URL-encoded in the address, JSON- or XML-escaped in the body, line breaks removed in headers. A message with quotes or line breaks no longer breaks an XML payload, and a placeholder in the address is no longer JSON-escaped. A value that itself contains a placeholder name is no longer replaced a second time.

- **WebHook — errors**: when the receiver refuses the call, the error shows its answer (the first 500 characters) along with the HTTP status. The log shows only the receiver's host, since Slack, Discord and Teams put the secret in the address.

- **WebHook — GET requests**: the body fields are hidden, since a GET request is sent without a body.

#### Fixed

- **Grids**: a group, once expanded, can be collapsed again.

- **Diagnostic — settings**: the node RRD time frame and PSI pressure thresholds changed from the settings page are now applied; before, the scan kept using the defaults. The Storage section no longer shows RRD and PSI fields, which the storage check never used.

- **Diagnostic — fewer false alarms and more accurate checks**:
  - The scan no longer stops halfway when a storage, node or guest does not answer: the missing part is reported and the rest of the scan is completed. When backups or other lists cannot be read, the checks depending on them are skipped instead of reporting "no backup" or "nothing configured".
  - Removed or corrected checks that fired by mistake: VLAN tag on a non VLAN-aware bridge (removed), unused network ports down, start on boot / locked / protection on containers, protected backups counted as old, container bind mounts, ZFS hot spares, disks behind RAID controllers, services not installed, CVEs already fixed in the installed version, and more.
  - Checks that could never fire now work: API tokens, quorum loss on node failure, failing replication jobs, pending kernel reboot, disabled storages still in use, empty pools.
  - Orphaned disks and backups now include containers and are reported once per deleted guest, and are skipped when the account cannot see every guest.
  - Numbers in the messages no longer depend on the server's regional settings (`80.9%` everywhere).
  - Compliance references checked against the official texts (DORA, NIS2, ENS, BSI C5).

- **Diagnostic — ignore rules to review**: the check for a VLAN tag on a non VLAN-aware bridge has been removed, and the *nesting without keyctl* warning is now an Info finding under a new code: ignore rules written for them no longer match anything. Rules matching numbers with a decimal comma (`80,9%`) must be rewritten with a dot.

- **Network diagram**: a guest on several bridges is linked to all of them, guests are listed by numeric id, Open vSwitch bridges are wired to their physical ports, and storages on older network configurations get their link to the bridge.

- **Guest details**: NVMe disks show their wearout like SSDs; the guest agent is recognised also when enabled with the `enabled=1` form; VM trunk ports are shown; options left at their Proxmox VE default (memory, swap, CPU type, OS type, …) show that default instead of an empty or zero value.

- **WebHook — stored secrets**: passwords, tokens and API keys of webhooks are stored encrypted, like the other credentials. Values saved before are kept and encrypted on the next save.

#### Internal

- Underlying components updated (Proxmox VE API library, Report and Diagnostic libraries, UI components, cache).

### Enterprise Edition

#### Added

- **Compliance — four new standards**, for a total of 18: ACN (Italian NIS2 measures), NIS2 Implementing Regulation (EU) 2024/2690, ISO 22301 (business continuity) and BSI IT-Grundschutz (Germany). They appear in the Compliance view and in the PDF and Excel reports.

- **AutoSnap — hook test**: a *Test* button in the hook editor sends the request right away, with sample values for the selected phase, and shows the answer.

#### Fixed

- **Workflow — guest configuration**: for containers, the activity that reads a guest's configuration now returns the real memory, OS type, tags and protection flag, instead of empty or zero values.

- **Help menu**: the red dot on the Help icon is back when a new version is available, and the separators between menu sections are drawn as lines again. The Help and notification buttons in the header are as narrow as the other icon buttons, with the counter moved to the corner so it no longer covers the bell.

- **NodeProtect — Git credentials**: the username and password changed in the Git settings are now saved; before, the change was lost. The password, like the secrets of AutoSnap hooks, is now stored encrypted; values saved before are kept and encrypted on the next save.

## [2.3.0] - 2026-09-24

This release focuses on **stability and interface polish**: pages no longer crash when data is refreshed in the background, grids keep expanded rows and groups the way you left them, severity colours are consistent and readable across light and dark themes, and notifications are clearer on every channel. Underlying components have been updated.

### Community Edition

#### Added

- **Orphan snapshots in Resources**: the guest *Snapshots* panel now also lists snapshots that still exist on the storage but are no longer known to Proxmox VE. They appear at the top, marked as orphans, with a warning showing how much space they take. They cannot be rolled back, edited or deleted from the UI. Like snapshot size calculation, this requires SSH access to the nodes.

- **Diagnostic — permission visibility check**: a new check reports up front which parts of the cluster the configured account can actually see. With a read-only role such as *PVEAuditor*, Proxmox VE hides data instead of returning an error, so guests could look unprotected; the backup checks now say they cannot see the volumes instead of reporting the opposite.

#### Changed

- **Notifications report what actually happened**:
  - Messages sent to webhooks and chat services no longer contain raw HTML tags; mail is sent as plain text.
  - The **Updater** notification now says how many security and regular updates are pending and how many guests need a reboot.
  - The test message names the notifier configuration that sent it, useful when several are set up.
  - Each module now sends its notifications with an appropriate severity instead of always as plain information.

- **Grids**: rows and groups tinted by severity use a lighter wash of the accent colour, so the text stays readable (also in the dark theme). Chart colours for severities now follow the application theme.

#### Fixed

- **AutoSnap**: the *Always* notification setting only sent failures, behaving like *On failure only*.
- **Webhook**: the default JSON template broke when the message contained quotes or line breaks (for example a job log).
- **Grids**: expanded detail rows collapsed and collapsed groups re-opened after a refresh or while scrolling; grids meant to open with groups collapsed now actually do.
- **Diagnostic**: findings about storages, users, backup jobs or cluster-wide settings no longer show a link that navigated away from the report.
- **Resources — Snapshots**: guests whose oldest snapshot had been removed by retention showed an empty snapshot list.
- **Profile**: the password fields are cleared (and hidden again) after a successful password change.
- **Dashboard**: the *Save* button is disabled until a name is entered; saving without a name raised an error.
- Pages listing AutoSnap jobs, backups, diagnostic scans, replications and system reports no longer crash when a background job updates the data while the page is open.
- Nested pools are now resolved correctly when selecting guests with `@pool-`.
- **Active Tasks panel**: a task finishing while the panel was open could appear twice and disconnect the page (yellow reconnect bar).
- **Tasks**: progress lines of long jobs such as System Report could fail to be saved, or get lost, while the job was running.

#### Documentation

- New section on orphan snapshots in Resources.
- Diagnostics: the page now lists all 14 compliance standards the checks are mapped to (previously only four were named), including GDPR, AgID, ENS and BSI C5.
- Corrected the PostgreSQL logging example in the advanced settings (the documented block did not produce any logging configuration), the Appearance settings, the search prefixes, the Proxmox VE permissions table (added AI Server and Bots) and the SSH verification in cluster settings.

### Enterprise Edition

#### Added

- **Daily Status widgets**: AutoSnap, Backup Analytics, Replication Analytics, Node Protect (folders) and System Report have a new widget showing successful and failed runs per day over the last 14 days.

- **UPS Monitor** and **Workflow** modules are now included in the released Enterprise Edition.

#### Changed

- **UPS Monitor — alerts**: notifications carry the right severity (error when a shutdown threshold is reached, warning for low battery or running on battery), are sent as plain text readable on every channel, and are translated.
- **System Logs**: the chart uses the application theme colours for log levels, matching badges and tinted rows elsewhere.
- **Portal**: orphan snapshots are not shown to tenants.
- The Apprise notifier image is pinned to a fixed version, so rebuilding the containers no longer changes the notifier under a working installation.

#### Fixed

- **Node Protect**: the *Git* entry showed "subscription required" instead of the actual Git configuration, because a placeholder appeared next to the real entry.
- **Workflow — Notify activity**: leaving the notifier list empty ("all") dropped the notification silently instead of sending it to every configuration.
- **Diagnostic — Compliance tab**: groups now stay collapsed or expanded as you left them, and findings with no page to open are shown as plain text instead of a link that left the report.
- **Grids**: expanded rows and groups are kept after a refresh in Portal tenants, Audit Logs, UPS devices and Node Protect Git.
- UPS dashboard, UPS devices and Node Protect Git pages no longer crash when a background job updates the data while the page is open.

## [2.2.0] - 2026-07-20

### Community Edition

#### Changed

- **Dashboard and Module Overview**: smoother widget grid. Drag, resize and rearrange widgets with better precision. Layout is more responsive across different screen sizes. Existing saved dashboards keep working as before — no manual step required.

- **Historical charts with zoom and pan**: on time-series charts (node/storage/VM metrics, Backup Analytics, Replication Analytics, Tasks) you can now zoom with the mouse wheel and drag to pan the view. Useful for inspecting a specific interval without changing filters.

#### Added

- **Dashboard widgets**: newly listed widgets on the Dashboard page (Cluster Usage Gauge Stacked, Cluster Usage Grid, Clusters Maps, VM/CT Locked, Nodes Status, Update Manager Status).

### Enterprise Edition

#### Changed

- **System Logs — Range selector**: below the 30-day log chart there is now a summary mini-chart with two draggable handles. Narrow the window down to days, hours or single spikes and the log grid below updates to match. Opens on the last 7 days by default.

- **Enterprise Edition — no license registration required**: the Subscription page is no longer shown in the menu. All enterprise modules run without activation.

## [2.1.0] - 2026-06-29

### Community Edition

#### Added

- **Built-in `system` user**. A locked-out, no-password user is now created on first boot and shown in the users list with the *Built-in* flag. It is the identity used by scheduled jobs and other non-interactive workflows (cron snapshots, retention cleanups, workflow timers), so the audit trail records *who actually did what* even when no person is logged in. It cannot be deleted, modified or used to sign in.

- **Scheduled job identity**. Any background job started from the web UI now runs as the user who clicked the button — manual *Snapshot now*, *Run scan now*, *Cleanup retention* and so on are recorded against your account, not as *anonymous*. Cron-driven jobs run as the new `system` user.

- **Module Overview pages**. Every module landing page now opens with the existing three-column intro plus a small dashboard built from the module's own widgets — no more *Overview* tabs that only show decorative text. Modules that have nothing meaningful to show as widgets (e.g. **Resources**) keep the textual overview as before.

- **Status badges**. The **Tasks** page status column now shows a coloured pill with an icon (Running / Completed / Failed / Cancelled / Abandoned), matching the level badges already used on the *(EE)* **Logs** page.

- **`adminctl self-update`** command refreshes your local `adminctl` script and the docker-compose YAML to match a chosen release without touching your data or `.env` configuration. By default it bumps to the latest release; you can also target a specific version. Existing files are backed up under `_backup/<timestamp>/` before being overwritten.

- **Diagnostic**
  - PDF and Excel reports are now available in the free edition (used to be Enterprise-only). The PDF lists every issue colour-coded by severity and a separate section with the issues you have chosen to ignore. The Excel workbook opens on a cover sheet and the data sheet has the autofilter already on.
  - **Compliance mapping**. Every check is tagged with the controls it covers across ISO/IEC 27001, EU NIS2, EU DORA and PCI DSS v4.0. The scan details now have a *Compliance* tab that groups findings by standard and control; the mapping is saved with the scan, so older reports stay consistent.
  - **Audit mode**: a new switch makes each passing check appear as a *Pass* result — handy when you have to prove that a control was actually verified, not only when it failed.
  - **Attach PDF / Excel** toggles in the notifier settings.
  - New checks: ZFS pool detail, LVM-thin metadata, S.M.A.R.T. temperature and SSD wearout thresholds, PSI Pressure thresholds (CPU / I/O / Memory), snapshot age and backup recency (RPO).
  - Each issue links straight to the relevant Proxmox VE documentation page (QEMU guest agent, VirtIO drivers, end-of-life OS).
  - The *Download* button on the Scans page is a split-button: one click for the PDF (default), use the arrow to pick the Excel.

- **Updater**
  - PDF and Excel reports in the free edition. The PDF lists every VM/CT and the status of its update scan; if some hosts failed the scan they get their own *Errors* page.
  - **Parallel scan** with a configurable *Max Parallel Requests* (1–50). A single host failing no longer stops the whole scan: the error is recorded against that host and the rest of the run continues.
  - Clicking *Scan* now starts the job immediately (the previous 5-second delay is gone).

- **System Report**
  - **Three output formats**: **Excel** (the existing one), **HTML** (a single browseable page with a table of contents and cross-section links) and **JSON** (machine-readable, with raw values and clean slugs).
  - **One zip file as result**, whatever the format: the zip contains the report itself plus the SVG network diagram.
  - The chosen format is **saved with the report** — opening an old report from the list reminds you how it was generated.
  - *Read-only* mode for the report dialog.
  - The page is now called *Reports* in the side menu (was *Scans*).

- **AutoSnap**
  - **Max Parallel** option per job (1–10) so snapshots within the same run can execute in parallel.

- **AI Server**
  - New MCP tools: list/download/delete ISO images, list templates, delete backups, delete arbitrary storage content, read cluster defaults (migration, bandwidth, HA, console).

- **Metrics Exporter**
  - **Per-cluster configuration** (used to be global): each cluster has its own on/off, its own collectors and its own cache windows.
  - Master *Enabled* switch, plus per-exporter and per-collector switches with editable cache seconds.
  - *Fast / Standard / Full* preset buttons to apply common configurations in one click.

- **Node Protect**
  - Folder backup runs now appear in *System → Tasks* alongside AutoSnap, System Report and Diagnostic, with a clickable deep link and per-node summary lines.
  - Updated default *Paths to backup*: dropped wildcard entries that didn't expand correctly when quoted, added `/var/spool/cron/crontabs` so per-user cron jobs are picked up.

- **Resources**
  - **Three new pages** in the side menu: **Networks**, **Disks** and **Partitions**, each with its own filters, grouping and card / list view switcher.
  - **Networks** has four tabs: *Nodes* (per-node bridges and bonds), *Guests* (per-VM virtual NICs), *SDN* (virtual networks) and *Diagram* (downloadable SVG topology view).
  - **Disks** lists the physical disks across the cluster with disk **Kind** (HDD / SSD / NVMe), model, serial, health and S.M.A.R.T. summary, with a drill-down per disk.
  - **Partitions** shows the partition / mount table per node.
  - **Snapshots** has a new *Trends* tab next to the snapshot list.
  - **Storages** reorganised into tabs: *Configuration*, *Storages*, *Usage* (with sub-tabs *By Storage* and *By VMs*).
  - **VMs** page renamed to **Guests** (it has always included LXC containers, the name now reflects the content).
  - **Card view** alongside the list view: every Guest / Node / Storage gets a card with status and key metrics. Toolbar with type / status / tag toggles, search and grouping.
  - Unified *Health Score* indicator: shows `N/A` when no data is available, with a tooltip explaining why.
  - Guest cards and lists show the operating system icon with a tooltip.

- **System**
  - The **Tasks** page has a search box in the toolbar that filters across Title, Cluster, Module, Phase, Created by and Last log.
  - The **Active Tasks** menu auto-clears the badge for failed / abandoned tasks once you have opened the panel.
  - **New user activation flow**: when an admin creates a user from the UI the new user receives an activation email with a password-reset link, instead of being created without a password.
  - **User unlock** command added to the CLI.
  - **Maintenance: *Compact (full)*** — new button that reclaims disk space after big cleanups. The action is opt-in only, asks for explicit confirmation, and is not run by *Fix All*.
  - **Help bundled with the app** — the user documentation is now included in the container and served locally.

- **Command palette**
  - The `ip:` filter now shows one IPv4 badge per VM (IPv6 and loopback addresses filtered out).

#### Changed

- **Issue cards on the *AutoSnap*, *Backup Analytics*, *Replication Analytics*, *Diagnostic* and *Resources* widgets**. The old "thumb up / thumb down + bullet list" has been redesigned: when everything is fine you see a green thumb up, when there are issues a red thumb down with a number, and when the module is not configured a neutral icon with an explanatory message. Hovering the number opens a compact list of the failing items; each row is clickable and takes you straight to the relevant detail page. With many rows the list scrolls inside itself instead of growing without bound.

- **Diagnostic issue labels**. The items on the *Diagnostic* widget used to be shown as internal paths that were hard to read. They now read *VM 100 on cc01 (cluster)*, *Node cc01 (cluster)*, *Storage ssd-pool (cluster)* and so on.

- **Diagnostic** settings screen redesigned with one accordion per area (Node, QEMU, LXC, Storage, Snapshot, Backup, CVE) and per-context thresholds clearly grouped.

- **Diagnostic** CVE checks now look up Proxmox VE specific CVEs in NVD using a CPE filter — wider coverage than the previous Debian-only tracker.

- **Resources** list view has been redesigned alongside the new card view (filters, grouping and search work consistently across both).

- **Task Tracking** Active Tasks panel and menu refresh row-by-row instead of reloading the whole grid, so the UI no longer flickers when tasks update.

#### Fixed

- **Built-in role permissions on existing installations**. Older databases were seeded with a flag that prevented built-in admin roles from matching specific resources (single VM, single node). The check appeared to *succeed* in role lists but silently *denied* the actual action — most visibly on scheduled AutoSnap deletes. The flag is now correctly set on fresh installs, and existing installations are realigned automatically on first boot.

- **Postgres log sink**. A typo in the column configuration was preventing the database log sink from writing any row — the file log kept working, so the issue went unnoticed. Database logging is now restored, and any future sink failure is surfaced on standard error instead of being swallowed.

- **PVE command result on permission denied / unauthorized / failure**. A regression in the result object construction caused an internal error to bubble up to the UI instead of the expected *Operation not permitted* / *Authentication required* message.

- **Backup Analytics scan**. The scan aborted in the middle when at least one vzdump task in the run had failed (e.g. a VM locked by a snapshot). The failed task is now imported correctly together with the successful ones.

- **KPI cards in the *Snapshot Statistics and Insights* widget** kept stacking vertically even on large screens. They now stay horizontal as soon as there is room for them.

- **Resources → Snapshots** and other grouped grids (AutoSnap status, BackupAnalytics, ReplicationAnalytics, Node Protect folders, Node storage contents): when grouping by more than one column the outer headers used to show only the group key and count. They now correctly aggregate totals (sizes, CPU and disk usage) from every nested level.

- **Diagnostic** Excel export no longer fails on scans where every issue is ignored. PDF reports wrap long text inside every cell instead of running off the right edge of the page.

- **System**
  - Cluster names with spaces no longer break navigation between module pages.
  - Validation messages inside edit dialogs are shown again.
  - Pages with many permission checks load fast, even on installations with many admin roles.
  - The "New Cluster" form no longer refuses the first submit because of a *PveName is required* error.
  - Release notes dialog shows only the current version, not earlier release candidates.
  - QEMU agent network cache refresh dropped from 30 to 15 minutes, so freshly attached NICs appear faster.
  - **Maintenance: *Reindex* and *Optimize* no longer touch other modules' data**. They used to run on the whole database once per installed module — so on an installation with sixteen modules each action ran sixteen times over everything. Now they run once per module, on that module's data only.

- **Notifier**: SMTP and WebHook editors refresh correctly when you pick a different notifier from the list.

- **Docker**: the container health check no longer marks the running container as unhealthy on every probe.

#### Compatibility note

- The default Apprise endpoint in the notifier settings changed from `http://localhost:8000` to `http://apprise:8000` to match the docker-compose service name. Existing notifiers keep the value you saved; new notifiers will use the new default.

#### Internal

- The background job system now uses a real PostgreSQL queue again (it had quietly fallen back to in-memory storage because of a conflicting registration from the workflow engine). Jobs survive process restarts, retries are durable, and the *Hangfire* dashboard shows real history.

### Enterprise Edition

#### Added

- **System / Logs** new native *Logs* page (replaces the Serilog UI integration) with server-side filtering by level, message and source, a stacked column chart of the last 30 days grouped by level, and a row-expansion for heavy fields (message template, exception, raw log event JSON) so the page stays fluid even on tables with millions of records.

- **Audit Logs** *Success* column now uses a coloured pill badge (*Success* / *Failed*).

- **Diagnostic — Compliance report**
  - The PDF gains one section per standard (ISO 27001, NIS2, DORA, PCI DSS). Every section starts on a new page with a short disclaimer, a summary table and a per-control detail block.
  - The Excel workbook gains one *Compliance - &lt;Standard&gt;* sheet per standard.
  - In the scan details page the *Compliance* tab is a real grid grouped by Standard → Control, with clickable links to the affected resources.
  - Colour coding on Gravity / Status cells is consistent between PDF and Excel.

- **System / Security**
  - When an admin creates a new user, a confirmation email is sent automatically. The new user receives an *activate your account* link instead of being stuck with no password.
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

- **Notifications** updated. Microsoft Teams is no longer supported; ntfy.sh users may need to update their URL.

#### Fixed

- **Background job permissions**. AutoSnap retention deletions and other scheduled jobs were failing with *Permission denied* because the job was running as *anonymous*. They now run as the `system` user with full admin rights and complete successfully.

- **Audit log writes**. Some audit log entries were silently dropped because the database constraint that links them to the user table was violated when no user could be resolved. The user is now always known (real user from the request, or `system` for jobs), and entries are written without errors.

- **Login activity in audit logs**. Successful and failed login attempts are now recorded with the attempted username (in the *Details* column), not as *anonymous*, making it possible to spot brute-force attempts on a specific account.

- **Edit User dialog** could fail on first render because of an internal type-visibility issue with the roles pick-list. The dialog opens reliably again.

- **Apprise notifier** *Render* page sometimes failed to load the catalog of available services because Apprise can serve either JSON or HTML on the same URL. We now explicitly ask for JSON and show a clear error if the endpoint replies with something else.

- **Diagnostic / Updater** PDF tables wrap long values correctly.

- **Workflow** editor no longer fails to start in tenant mode (the live-update channel authenticates correctly).

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

