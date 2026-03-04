# Resource Scheduler (DRS)

<span class="ee"></span> Dynamic Resource Scheduler that automatically balances VM load across Proxmox VE nodes.

## Overview

**DRS (Dynamic Resource Scheduler)** continuously monitors resource usage across all Proxmox VE nodes and automatically migrates VMs to maintain an optimal load balance throughout the cluster.
It runs on a configurable schedule and evaluates CPU, memory, disk, and PSI pressure metrics to determine whether migrations are necessary.

---

## Key Features

- **Automatic VM Migration** — Balances load across nodes based on configurable metrics
- **Maintenance Mode** — Automatically evacuates all VMs from nodes entering maintenance, ensuring zero downtime
- **Affinity Rules** — Keep related VMs co-located on the same node for low-latency communication
- **Anti-Affinity Rules** — Spread VMs across different nodes for fault isolation and high availability
- **Pinned VMs** — Exclude specific VMs from migration to honour licensing or hardware constraints
- **Dry Run** — Simulate the balancing cycle without performing actual migrations for safe testing
- **HA Integration** — Respects Proxmox HA group restrictions; restricted groups constrain DRS migrations to the listed nodes only

---

## Balancing Methods

| Method | Description |
|--------|-------------|
| **Memory** | Current memory usage % (snapshot) |
| **CPU** | Current CPU usage % (snapshot) |
| **MemoryRrd** | Average memory usage % over the last hour (RRD). Avoids transient spikes. |
| **CpuRrd** | Average CPU usage % over the last hour (RRD). Avoids transient spikes. |
| **MemoryAssigned** | Allocated maxmem % per node. Production-safe, based on configured resources. |
| **CpuAssigned** | Allocated vCPU count % per node. Production-safe, based on configured resources. |
| **Disk** | Local (non-shared) storage utilisation % across nodes |
| **PSI** | Linux Pressure Stall Information — the most accurate real-world load indicator, detecting CPU, memory, and I/O pressure before performance degrades. Requires PVE 9+. |

---

## Configuration

| Setting | Description |
|---------|-------------|
| **Enabled** | Enable or disable automatic scheduling |
| **Live Migration** | Use live migration (online) when migrating VMs |
| **Dry Run** | Simulate without executing actual migrations |
| **Schedule (Cron)** | Configurable cron expression for automatic runs |
| **Balancing Method** | The metric used to evaluate and balance load |
| **Balanciness Delta (%)** | Minimum load difference between nodes required to trigger a migration |
| **Max Parallel Migrations** | Maximum number of concurrent VM migrations |
| **Max Disk Usage (%)** | Skip migration if destination node disk usage exceeds this threshold (0 = disabled) |

### PSI Thresholds (PVE 9+)

When using the PSI method, configure per-metric thresholds:

| Threshold | Description |
|-----------|-------------|
| **Full (%)** | All tasks stalled — critical pressure |
| **Some (%)** | At least one task stalled — elevated pressure |
| **Spike (%)** | Instantaneous peak pressure |

---

## Pages

| Page | Description |
|------|-------------|
| **Overview** | Summary of DRS capabilities and balancing methods |
| **DRS** | Configuration, node management, migration history, and manual run |

### DRS Page — Tabs

| Tab | Description |
|-----|-------------|
| **Settings** | Scheduler configuration, balancing method, VM constraints, enabled/disabled/maintenance nodes |
| **HA Groups** | Proxmox HA groups (read-only); restricted groups limit DRS migrations to specified nodes |
| **Last Migrations** | History of DRS runs with migration details (VM, source/destination node, load, result) |
