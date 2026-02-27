# Resources

Infrastructure resource management and visualization for Proxmox VE clusters.

## Overview

Real-time cluster overview with VM states, node health, and resource utilization.

## Enterprise Edition Features

The Enterprise Edition includes additional columns for enhanced resource visibility:

- **Hostname**: Display the hostname for each VM/container
- **OS Info**: Operating system information for better identification and management

<div class="grid cards" markdown>

-   :material-dashboard:{ .lg .middle } **System Health**

    ---

    View CPU, memory, and storage usage across cluster nodes.

-   :material-server:{ .lg .middle } **VM Status**

    ---

    View VM and container running, stopped, and error states.

-   :material-database:{ .lg .middle } **Storage Monitoring**

    ---

    Monitor storage capacity and performance indicators.

-   :material-network:{ .lg .middle } **Network Metrics**

    ---

    View connectivity and bandwidth utilization metrics.

-   :material-play-circle:{ .lg .middle } **Quick Actions**

    ---

    Start, stop, restart VMs from status view.

-   :material-chart-line:{ .lg .middle } **Performance Data**

    ---

    Real-time CPU, memory, and I/O statistics.

-   :material-console:{ .lg .middle } **Console Access**

    ---

    Open NoVnc, Xterm.js, or Spice console directly from the resource view.

</div>

## Console Limitations

### Node Console (NoVnc, Xterm.js)

Node console access requires the WEB API user to be a **PAM user** (e.g. `root@pam`).

Users authenticated via other realms (PVE, LDAP, etc.) cannot open a node console because Proxmox VE node consoles require OS-level authentication. The console button will be disabled if the WEB API user is not a PAM user.

### VM/Container Console (Qemu, LXC)

VM and container console access requires the WEB API connection to use **Credential** authentication (username/password).

The console button will be disabled when the WEB API is configured with **API Token** authentication, as Proxmox VE requires a user session to open a VM console.
