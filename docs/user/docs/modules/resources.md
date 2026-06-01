# :material-check-decagram: Resources <span class="scope" data-scope="per-cluster"></span>

Real-time, cross-cluster inventory and operations on every kind of Proxmox VE resource: clusters, nodes, guests, storage, networks, disks, partitions and snapshots.

## Features

<div class="grid cards" markdown>

-   :material-magnify:{ .lg .middle } **Search**

    ---

    Search box on Guests, Snapshots and other grids — filters across name, description, IP, hostname, tags simultaneously.

-   :material-view-grid:{ .lg .middle } **Card View**

    ---

    On Guests and Storages, switch from grid to card layout for a glance-at-everything overview. Cards show health score, status badges, key metrics and the same quick actions as the grid.

-   :material-speedometer:{ .lg .middle } **Health Score**

    ---

    Unified `HealthScore` indicator (badge or gauge) computed from CPU, memory, disk usage and resource type, with a contextual tooltip explaining the score.

-   :material-play-circle:{ .lg .middle } **Quick Actions**

    ---

    Start, stop, restart, shutdown, snapshot or open a console directly from any row — without leaving the page.

-   :material-console:{ .lg .middle } **Console Access**

    ---

    Open NoVnc, Xterm.js or Spice console for nodes, VMs and containers from the resource view.

-   :material-link-variant:{ .lg .middle } **Cross-references**

    ---

    Click a node name, VM ID, storage or snapshot to jump to its detail page; the cluster context is preserved automatically.

-   :material-graph:{ .lg .middle } **Network diagram**

    ---

    The Networks → Diagram tab renders an interactive SVG topology of nodes, bridges, VNets and guests (powered by [`cv4pve-report`](https://github.com/Corsinvest/cv4pve-report)) — savable for documentation.

</div>

## Why

Why this view when PVE already shows resources per cluster?

<div class="why-grid" markdown>

<div markdown>
!!! tip "Every cluster at once"
    PVE's UI is per-cluster. Resources lists VMs, nodes, storage and snapshots from **every** configured cluster in one filterable grid.
</div>

<div markdown>
!!! success "Filter, sort, group, search"
    RadzenDataGrid gives multi-column sort, grouping, column picker and free-text search — find that one VM with `prod` in tags across 200 across clusters.
</div>

<div markdown>
!!! info "Quick actions from the list"
    Start, stop, snapshot, open console without ever opening the per-VM page — straight from the row.
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
- **Guests** — VMs and containers from every cluster, with running/stopped state, owner node, tags, IPs, lock state and quick actions (start/stop/console/snapshot)
- **Storages** — storage definitions across the cluster, type, status, usage
- **Networks** — tabbed view: **Nodes** (bridges/bonds) · **Guests** (per-VM NIC config) · **SDN** (zones, VNets) · **Diagram** (interactive SVG topology)
- **Disks** — physical disks per node with model, size, S.M.A.R.T. health, vendor, **Kind** column (HDD / SSD / NVMe), wearout
- **Partitions** — partitions and mount points (read via QEMU Guest Agent)
- **Snapshots** — all snapshots in the cluster with creation date, description, parent, optional **size on disk**

## <span class="ee"></span> Enterprise Additions

Enterprise enables extra columns and detail data on Guests and other grids:

- **Hostname** — collected via QEMU Guest Agent, useful when the VM name and the in-guest hostname differ
- **OS Info** — OS family, distribution and version
- Additional widgets in the [Dashboard](dashboard.md) feeding from Resources data

## Console Limitations

!!! warning "Node console requires a PAM user"
    Node console (NoVnc, Xterm.js) requires the WEB API user to be a **PAM user** (e.g. `root@pam`). Users authenticated via other realms (PVE, LDAP, …) cannot open a node console because Proxmox VE requires OS-level authentication. The console button is disabled if the WEB API user is not a PAM user.

!!! warning "VM/CT console requires Credential authentication"
    VM and container console (QEMU, LXC) requires the WEB API to use **Credential** authentication (username/password). The console button is disabled when the WEB API is configured with **API Token** authentication, because Proxmox VE requires a user session to open a VM console.
