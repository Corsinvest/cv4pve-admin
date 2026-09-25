# :material-check-decagram: Resources <span class="scope" data-scope="per-cluster"></span>

Real-time inventory and operations, for the selected cluster, on every kind of Proxmox VE resource: clusters, nodes, guests, storage, networks, disks, partitions and snapshots.

## Features

<div class="grid cards" markdown>

- :material-magnify:{ .lg .middle } **Search**

    ---

    Search box on Guests, Snapshots and other grids — filters across name, description, tags and the other text columns simultaneously (hostname too when **Show OS Info** is on).

- :material-view-grid:{ .lg .middle } **Card View**

    ---

    On Nodes, Guests and Storages, switch from grid to card layout for a glance-at-everything overview. Cards show health score, status badges, key metrics and the same quick actions as the grid.

- :material-speedometer:{ .lg .middle } **Health Score**

    ---

    Unified `HealthScore` indicator (badge or gauge) computed from CPU, memory, disk usage and resource type, with a contextual tooltip explaining the score.

- :material-play-circle:{ .lg .middle } **Quick Actions**

    ---

    Start, shutdown, reboot, stop, reset, pause, unlock or open a console directly from any row — without leaving the page. Snapshots are managed from the row's expanded detail.

- :material-console:{ .lg .middle } **Console Access**

    ---

    Open NoVnc, Xterm.js or Spice console for nodes, VMs and containers from the resource view.

- :material-link-variant:{ .lg .middle } **Cross-references**

    ---

    Click a node name, VM ID, storage or snapshot to jump to its detail page; the cluster context is preserved automatically.

- :material-graph:{ .lg .middle } **Network diagram**

    ---

    The Networks → Diagram tab renders an interactive SVG topology of nodes, bridges, VNets and guests (powered by [`cv4pve-api-dotnet`](https://github.com/Corsinvest/cv4pve-api-dotnet)) — savable for documentation.

</div>

## Why

Why this view when PVE already shows resources per cluster?

<div class="why-grid" markdown>

<div markdown>
!!! tip "The whole cluster at once"
    Resources lists VMs, nodes, storage and snapshots of the **whole** selected cluster in one filterable grid.
</div>

<div markdown>
!!! success "Filter, sort, group, search"
    RadzenDataGrid gives multi-column sort, grouping, column picker and free-text search — find that one VM with `prod` in tags among 200.
</div>

<div markdown>
!!! info "Quick actions from the list"
    Start, stop, open console without ever opening the per-VM page — straight from the row.
</div>

<div markdown>
!!! warning "Network at a glance"
    The Networks tab gives bridges/bonds/SDN/diagram in one place — what PVE only shows split between per-node tabs.
</div>

</div>

## Sections

- **Overview** — high-level dashboard of clusters, nodes, guests and storage
- **Cluster** — per-cluster summary with node counts, guest counts and resource roll-ups
- **Nodes** — all cluster nodes with CPU/Memory/Disk usage, status, kernel, uptime, hostname
- **Guests** — VMs and containers of the cluster, with running/stopped state, owner node, tags, lock state and quick actions (start/stop/console); the **Show OS Info** toggle adds hostname and OS family/version (read via QEMU Guest Agent)
- **Storages** — storage definitions across the cluster, type, status, usage
- **Networks** — tabbed view: **Nodes** (bridges/bonds) · **Guests** (per-VM NIC config) · **SDN** (zones, VNets) · **Diagram** (interactive SVG topology)
- **Disks** — virtual disks of every VM/CT, grouped by guest: kind, storage, file, size, cache, backup flag, unused, mount point, passthrough, format
- **Partitions** — partitions and mount points (read via QEMU Guest Agent)
- **Snapshots** — all snapshots in the cluster with host, VM ID, name and date; with snapshot size calculation <span class="ee"></span> also storage, disk, **size on disk**, replication flag and a **Trends** tab

## <span class="ee"></span> Enterprise Additions

Enterprise enables snapshot size data on Resources:

- **Snapshot size** — size on disk in the Snapshots grid and in a guest's Snapshots tab, the **Snapshots Usage** card on Cluster and the snapshot gauge in the cluster summary
- **Orphan snapshots** — see below

### Orphan snapshots

A snapshot removed in Proxmox VE can survive on the underlying storage: the removal does not always propagate to the dataset, and the space stays occupied. Proxmox VE no longer lists it, so nothing in its UI reveals it — the disk simply looks fuller than the snapshots explain.

Opening a guest's **Snapshots** tab from Resources lists these first, marked as orphans, with a warning summing the space they hold. They cannot be rolled back, edited or deleted from here, since Proxmox VE has no snapshot to act on: removing one means working on the storage directly, on the node.

!!! note "Requires SSH"
    Orphan detection compares what the storage holds against what Proxmox VE reports, so it needs the same [SSH credentials](../configuration/admin-area/clusters.md#ssh-configuration) as snapshot size calculation. Without them, neither is shown.

Sizes are read per storage type. On ZFS the reported size is the space the snapshot actually occupies; on other backends it is currently an approximation.

## Console Limitations

!!! warning "Node console requires a PAM user"
    Node console (NoVnc, Xterm.js) requires the WEB API user to be a **PAM user** (e.g. `root@pam`). Users authenticated via other realms (PVE, LDAP, …) cannot open a node console because Proxmox VE requires OS-level authentication. The console button is disabled if the WEB API user is not a PAM user.

!!! warning "VM/CT console requires Credential authentication"
    VM and container console (QEMU, LXC) requires the WEB API to use **Credential** authentication (username/password). The console button is disabled when the WEB API is configured with **API Token** authentication, because Proxmox VE requires a user session to open a VM console.
