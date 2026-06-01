# <span class="ee"></span> :material-graph: Workflow <span class="scope" data-scope="per-cluster"></span>

Visual designer for event-driven automation against Proxmox VE — built on [Elsa Workflows](https://elsa-workflows.github.io/). Compose snapshot, backup, migration, power and notification activities into reusable workflows.

## Features

<div class="grid cards" markdown>

- :material-cursor-pointer:{ .lg .middle } **Drag-and-drop Designer**

    ---

    Build workflows visually in Elsa Studio — embedded directly in cv4pve-admin, no external tool.

- :material-server-network:{ .lg .middle } **PVE Activities**

    ---

    Pre-built activities for guests (power, backup, snapshot, migrate, clone, resize), nodes (shutdown/reboot, inventory, RRD), storages (inventory, content, RRD), HA (manage state) and replications.

- :material-bell-ring:{ .lg .middle } **Admin Activities**

    ---

    Built-in `Notify` activity to send messages through any configured Notifier — Slack, Telegram, email, webhook.

- :material-code-tags:{ .lg .middle } **C# and JavaScript**

    ---

    Inline expressions in C# or JavaScript anywhere a value is accepted — branching, filtering, formatting.

- :material-clipboard-list:{ .lg .middle } **History & Audit**

    ---

    Every workflow run is recorded with status, inputs and timing — replay or inspect from the History tab.

- :material-account-group:{ .lg .middle } **Multi-tenant**

    ---

    Workflows are isolated per cluster (tenant) — designing in one cluster does not leak into another.

</div>

## Why

Why a workflow engine when scripts and cron already work?

<div class="why-grid" markdown>

<div markdown>
!!! tip "No scripts to maintain"
    Drag-and-drop, no Bash, no PowerShell, no SDK. Tasks that used to live in a wiki page become a workflow.
</div>

<div markdown>
!!! success "Branching and error paths"
    Conditions, loops, retries, error handling — without writing a state machine. The designer makes the control flow obvious.
</div>

<div markdown>
!!! info "Cluster-aware activities"
    Activities know about your nodes, VMs, storages, HA groups — pickers populated from the live cluster, no IDs to copy-paste.
</div>

<div markdown>
!!! warning "Auditable runs"
    Every execution recorded with inputs, outputs and timing — answer "when did this happen and what did it do?" without digging through logs.
</div>

</div>

## Sections

- **Workflows** — open Elsa Studio to design, edit and publish workflows for the current cluster
- **History** — list of past workflow runs with status, duration and per-step detail

## Available Activities

??? note tools "Show all PVE activities"

    **Guest — Inventory**

    | Activity | What it does |
    |----------|--------------|
    | `GetGuests` | List VMs and CTs with status, node, tags |
    | `GetConfig` | Read full VM/CT configuration (CPU, memory, disks, network) |
    | `GetRrdData` | Historical RRD metrics for a VM/CT |

    **Guest — Operations**

    | Activity | What it does |
    |----------|--------------|
    | `PowerAction` | Start / Stop / Shutdown / Reset / Suspend |
    | `Backup` | Trigger vzdump backup with compression and mode |
    | `Migrate` | Live or offline migration to another node |
    | `Clone` | Clone a VM/CT |
    | `ConvertToTemplate` | Convert a VM/CT to template |
    | `Resize` | Resize a virtual disk |

    **Guest — Snapshots**

    | Activity | What it does |
    |----------|--------------|
    | `Create` | Create a snapshot |
    | `Delete` | Delete a snapshot |
    | `Rollback` | Rollback to a snapshot |
    | `Get` | List/inspect snapshots |
    | `Update` | Update snapshot description |

    **Node**

    | Activity | What it does |
    |----------|--------------|
    | `GetNodes` | List nodes with status |
    | `GetReplications` | List replication jobs |
    | `GetRrdData` | Historical RRD metrics for a node |
    | `ShutdownReboot` | Shutdown or reboot a node |

    **Storage**

    | Activity | What it does |
    |----------|--------------|
    | `GetStorages` | List cluster storages |
    | `GetStorageContents` | List content on a storage (ISOs, backups, templates) |
    | `GetRrdData` | Historical RRD metrics for a storage |

    **Cluster**

    | Activity | What it does |
    |----------|--------------|
    | `ManageHA` | Manage HA resources (add, remove, set state) |
    | `GetReplications` | Cluster-wide replication inventory |

    **Utilities**

    | Activity | What it does |
    |----------|--------------|
    | `WaitForTask` | Wait for a Proxmox task to complete |
    | `Notify` | Send a message through a configured Notifier (Slack, Telegram, email, webhook) |
    | `Filter` | Filter a collection by expression |

!!! info "Persistence"
    Workflow definitions and runtime state are stored in the `workflow_elsa` Postgres schema — separate from the cv4pve-admin schema.
