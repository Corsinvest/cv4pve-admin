# :material-stethoscope: Diagnostics <span class="scope" data-scope="per-cluster"></span>

Automated health checks and infrastructure diagnostics for Proxmox VE, built on the [`cv4pve-diag`](https://github.com/Corsinvest/cv4pve-diag) library. Each issue is reported with a gravity (`Info` / `Warning` / `Critical`), a context (cluster, node, storage, qemu, lxc) and a sub-context that pinpoints what to look at.

## Features

<div class="grid cards" markdown>

--8<-- "_includes/feature-cron.md"

-   :material-alert-circle:{ .lg .middle } **Issue Detection**

    ---

    Detects misconfigurations, hardware problems, end-of-life software, snapshot/backup hygiene and resource pressure across **cluster, nodes, storage, QEMU VMs and LXC containers**.

-   :material-tune-variant:{ .lg .middle } **Tunable Thresholds**

    ---

    Per-context thresholds (CPU, Memory, Network, Health Score, PSI pressure) editable from the settings UI. Separate profiles for Node, Qemu, Lxc and Storage. Optional checks for S.M.A.R.T. disk attributes (temperature, SSD wearout), ZFS pool detail and LVM-thin metadata.

-   :material-camera-burst:{ .lg .middle } **Snapshot & Backup Hygiene**

    ---

    Per-VM checks for snapshot count and age, backup recency (RPO violation) and stale backup files wasting storage. Each check toggleable, thresholds editable.

-   :material-shield-bug:{ .lg .middle } **CVE Tracking**

    ---

    Optional NVD lookup for Proxmox VE specific CVEs via CPE filter. Filter by minimum CVSS v3 score (default 7.0 = HIGH + CRITICAL only).

-   :material-block-helper:{ .lg .middle } **Ignored Issues**

    ---

    Hide known/accepted issues from the active result — they still appear in a dedicated section at the bottom of the report for full traceability.

--8<-- "_includes/feature-parallel-scan.md"

--8<-- "_includes/feature-export-pdf-excel.md"

-   :material-link-variant:{ .lg .middle } **Help Links**

    ---

    Each issue can link to the relevant Proxmox documentation page (e.g. QEMU Guest Agent, VirtIO drivers, end-of-life OS tracking).

-   <span class="ee"></span> :material-chart-box:{ .lg .middle } **Executive Summary**

    ---

    Enterprise PDF adds a one-page Executive Summary at the top: issue counts by gravity (Critical / Warning / Info) and the top 5 critical issues — useful for management or MSP customer reports.

-   <span class="ee"></span> :material-shield-check:{ .lg .middle } **Compliance Reports**

    ---

    Each diagnostic check carries its mapping to normative controls (ISO 27001, NIS2, DORA, PCI DSS). Enterprise PDF appends one section per standard — a new page that starts with a summary table (control / title / status) and continues with the failing checks grouped by control. Excel exports get one extra sheet per standard with the same data flattened for filtering and pivoting.

--8<-- "_includes/feature-notifier.md"

</div>

## Why

Why scan automatically when you can check things by hand?

<div class="why-grid" markdown>

<div markdown>
!!! tip "All clusters in one report"
    A scan covers every node, storage, QEMU VM and LXC container of the cluster — you don't open one PVE UI per node and click around.
</div>

<div markdown>
!!! success "Every issue tagged for triage"
    Each finding carries gravity (Info / Warning / Critical), context, sub-context — sort by gravity, filter by context, fix the right things first.
</div>

<div markdown>
!!! info "Accept the noise, keep the signal"
    Known false positives go to the Ignored list — they keep showing in the report (audit trail) but stop cluttering the active scan.
</div>

<div markdown>
!!! warning "Compliance hand-off"
    Persisted scan history + PDF / Excel reports — hand them to auditors or customers without screenshotting the UI.
</div>

</div>

## Sections

- **Scans** — browse the history of scans, expand a row to drill into per-issue detail, download the report as PDF or Excel, trigger a new on-demand scan
- **Ignored Issues** — manage the list of issues you want to hide from active results (still shown in the report's Ignored section)

## Compliance reports <span class="ee"></span>

Each check in the `cv4pve-diag` library declares which normative controls it covers (a single check can satisfy more than one control, even across different standards). This mapping is persisted with the scan, so historic reports stay consistent even when the catalog evolves.

When you export an Enterprise PDF, the report includes — after the standard issues section — **one section per standard** that produced at least one mapped finding. Each section starts on a new page and contains:

- A compact **disclaimer** at the top (every section, so any single page extracted in isolation still carries the audit-scope notice).
- A **summary table** with one row per control: `Control` · `Title` · `Critical` · `Warning` · `Info` · `Ok` · `Status`. The numeric columns count findings mapped to that control by gravity; `Status` summarises the worst gravity present (`Fail` for any Critical, `Warning` if no Critical, `Info` if only Info, `Pass` if only Ok results).
- A **detail block** for each control, listing the underlying findings with `Code`, `Context`, `Resource`, `Description`, `Status`.

Excel exports get **one extra sheet per standard** with two tables: a `Summary` (one row per control, identical to the PDF summary) and a `Details` (one row per finding × control mapping, flattened for filtering and pivoting). Each sheet starts with the same disclaimer note.

Gravity / Status cells use a pastel colour palette consistent between PDF and Excel (Critical/Fail red, Warning orange, Info blue, Ok/Pass green). The Excel formatting is applied as a conditional rule on any column named `Gravity` or `Status`, so user-driven sort/filter keeps the colours.

Currently supported standards: **ISO/IEC 27001:2022**, **EU NIS2**, **EU DORA**, **PCI DSS v4.0**. Sections only appear for standards with actual mappings — empty standards are skipped.

!!! tip "Audit mode"
    Enable **Include OK results (audit mode)** in the General settings to also emit a `Pass` result for every check that succeeds. Useful when auditors want evidence that controls were *verified*, not only when they failed.

!!! warning "Disclaimer"
    The compliance mapping is automated and technical only — it covers the subset of each standard that can be verified from the Proxmox VE state. Policies, training, supplier management, physical security and other organisational controls are out of scope. A passing check confirms only that the specific automated rule passed; full conformity usually requires manual evidence (policies, procedures, evidence of operation). This report does not constitute a formal audit or certification.

## Settings

??? note settings "Show all settings"

    **General**

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **Enabled schedulation** | off | Master on/off switch for the scheduled scan |
    | **Cron Expression** | `0 6 * * *` [:material-open-in-new:](https://crontab.guru/#0_6_*_*_*){target=_blank title="Open on crontab.guru"} | When the scheduled scan runs |
    | **Keep** | 30 | Max number of past scan results to retain |
    | **Notifier Configurations** | – | List of Notifier configurations to deliver the report to |
    | **Attach PDF report** | on | Attach the PDF report (with Executive Summary and Compliance sections) to scheduled notifications |
    | **Attach Excel report** | off | Attach the Excel workbook (Issues, Ignored, Compliance sheets) to scheduled notifications |

    **API**

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **Max Parallel Requests** | 5 | Max concurrent API calls (1 = sequential) |
    | **API Timeout** | 0 | API timeout in seconds (0 = use the client default) |
    | **Include OK results (audit mode)** | off | Also emit a result for every check that passes (Gravity = Ok). Useful for audit-style reports proving controls were verified, not only violated |

    **Host thresholds** — Node / QEMU / LXC each have their own profile (Node extends Host with extra checks)

    | Threshold | Warning | Critical |
    |-----------|---------|----------|
    | **CPU usage (%)** | 70 | 85 |
    | **Memory usage (%)** | 70 | 85 |
    | **Network usage (%)** | – | – |
    | **Health Score** (Host default) | 70 | 50 |
    | **Health Score** (QEMU / LXC default) | 60 | 40 |
    | **PSI Pressure CPU (%)** — host | 50 | 80 |
    | **PSI Pressure CPU (%)** — node | 40 | 70 |
    | **PSI Pressure I/O full (%)** — host | 20 | 50 |
    | **PSI Pressure I/O full (%)** — node | 10 | 30 |
    | **PSI Pressure Memory full (%)** — host | 10 | 30 |
    | **PSI Pressure Memory full (%)** — node | 5 | 15 |

    **Node-only thresholds**

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **Max vCPU ratio** | 4.0 | Warn when total vCPUs / physical CPUs exceeds this ratio |
    | **Consolidation CPU threshold (%)** | 10 | Below this, the node is considered underutilised |
    | **Consolidation Memory threshold (%)** | 20 | Below this, the node is considered underutilised |
    | **S.M.A.R.T. checks → Enabled** | off | Per-disk SMART attribute scan (1 API call per disk per node) |
    | **S.M.A.R.T. → Temperature (°C)** | Warn 55 / Crit 65 | Disk temperature thresholds (set Warning to 0 to disable) |
    | **S.M.A.R.T. → SSD Wearout (%)** | Warn 70 / Crit 85 | Percentage of SSD life consumed before warning |
    | **NodeStorage → ZFS detail** | off | Deeper ZFS pool status check |
    | **NodeStorage → LVM-thin metadata** | on | LVM-thin metadata usage check (full metadata pool causes data corruption) |

    **Snapshot checks**

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **Enabled** | on | Toggle the per-VM/CT snapshot scan (skip the API call when off) |
    | **MaxCount** | 10 | Warn when a VM/CT has more than this many snapshots (0 = disabled) |
    | **MaxAgeDays** | 30 | Warn when the oldest snapshot exceeds this age in days (0 = disabled) |

    **Backup checks**

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **Enabled** | on | Toggle backup hygiene checks (skip API calls when off) |
    | **MaxAgeDays** | 60 | Warn about backups older than this on storage (0 = disabled) |
    | **RecentDays** | 7 | Warn when no backup exists within this window (RPO violation, 0 = disabled) |

    **CVE checks**

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **NVD** | off | Lookup NVD for Proxmox VE specific CVEs (CPE filter) |
    | **Min CVSS v3 score** | 7.0 | Report only CVEs at or above this score (0 = report all) |
