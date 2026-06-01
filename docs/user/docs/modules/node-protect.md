# :material-shield-check: Node Protect <span class="scope" data-scope="per-cluster"></span>

Periodic backup of selected Proxmox VE node configuration files — `/etc`, `/var/lib/pve-cluster`, crontabs, SSH keys, custom scripts — so you can recover from a failed node or accidental misconfiguration.

## Features

<div class="grid cards" markdown>

-   :material-folder-multiple:{ .lg .middle } **Targeted path list**

    ---

    Pick exactly which files and directories to protect — defaults cover the essentials (`/etc/.`, `/var/lib/pve-cluster/.`, `/root/.ssh`, …) and you can add your own.

--8<-- "_includes/feature-cron.md"

-   :material-folder-zip:{ .lg .middle } **Local folder destination**

    ---

    Each run is stored as a versioned snapshot in a local folder, capped by the **Keep** retention setting — older runs are pruned automatically.

-   <span class="ee"></span> :material-source-branch:{ .lg .middle } **Git destination**

    ---

    Push every backup as a Git commit to a remote repository — full configuration history with diffs, blame, code review and the rest of the Git ecosystem.

-   :material-chart-line:{ .lg .middle } **Size widgets on the Dashboard**

    ---

    Drop the **Folder Size** widget (CE) — or the additional **Git Size** widget (EE) — on any Dashboard to track backup growth per node at a glance.

--8<-- "_includes/feature-task-tracker.md"

</div>

## Why

Why a config backup module when you already do disk / VM backups?

<div class="why-grid" markdown>

<div markdown>
!!! tip "Recover a node, not a whole disk image"
    Need to restore a single hook in `/etc/pve` or one SSH key? Grab it from the latest backup — no need to mount a full PBS dump or boot a recovery image.
</div>

<div markdown>
!!! success "Survive a node reinstall"
    Wiping a node and starting fresh: copy back `/etc`, `/var/lib/pve-cluster`, your crontabs and SSH keys — you're back online in minutes, not hours.
</div>

<div markdown>
!!! info "Know what changed, when"
    With the Git destination every change is a commit you can diff. Find out who edited `corosync.conf` and when — no more silent drift across nodes.
</div>

<div markdown>
!!! warning "Catches what PBS doesn't"
    Proxmox Backup Server is great for VMs, not for node-level config. Node Protect fills the gap that PBS leaves intentionally open.
</div>

</div>

## Sections

- **Folder** — browse, download and restore backups stored in the local folder destination
- <span class="ee"></span> **Git** — browse commits, diffs and restore files from the Git destination

## Settings

??? note settings "Show all settings"

    **General**

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **Enabled schedulation** | off | Master on/off switch for the scheduled backup |
    | **Cron Expression** | `0 6 * * *` [:material-open-in-new:](https://crontab.guru/#0_6_*_*_*){target=_blank title="Open on crontab.guru"} | When the scheduled backup runs (default: 06:00 daily) |
    | **Directory or file** | preset list | Newline-separated paths to back up (`/etc/.`, `/var/lib/pve-cluster/.`, `/root/.ssh`, ...). Edit to suit your node layout |

    **Folder destination**

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **Enabled** | off | Enable local folder backup destination |
    | **Keep** | 30 | Maximum number of past backups to retain in the folder |

    **<span class="ee"></span> Git destination**

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **Enabled** | off | Enable Git remote as backup destination — each scheduled backup commits and pushes the protected files |
    | **Remote Url** | – | URL of the remote Git repository (`https://…`, `git@…`) |
    | **Remote Branch Name** | – | Branch name to push commits to |
    | **Credential** | – | Username / password (HTTPS) or SSH key (SSH) used to authenticate with the remote |
    | **Display Name / Email** | – | Author identity recorded on every commit |
    | **Ignore** | preset (`/etc/pve/nodes/*/lrm_status`, `/etc/pve/.rrd`) | `.gitignore` content — paths matching these globs are not committed |

--8<-- "_includes/requirements-ssh.md"
