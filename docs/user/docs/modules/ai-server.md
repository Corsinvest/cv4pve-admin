# AI Server

AI-powered infrastructure management for Proxmox VE.

## Overview

Model Context Protocol (MCP) endpoint that enables AI assistants to interact with Proxmox VE. Connect AI tools to query cluster status, manage VMs, and automate tasks.

<div class="grid cards" markdown>

-   :material-robot:{ .lg .middle } **AI Assistant Integration**

    ---

    Manage Proxmox infrastructure through natural language via AI assistants.

-   :material-database-eye:{ .lg .middle } **Data Access**

    ---

    AI tools can query VM status, cluster health, and resource usage.

-   :material-auto-fix:{ .lg .middle } **Automated Tasks**

    ---

    AI assistants can perform VM management, snapshot creation, and monitoring.

-   :material-api:{ .lg .middle } **MCP Protocol**

    ---

    Uses Model Context Protocol for AI integration.

-   :material-link-variant:{ .lg .middle } **AI Tool Connection**

    ---

    Connect AI assistants to Proxmox environments.

-   :material-shield-lock:{ .lg .middle } **Access Control**

    ---

    Controlled API exposure for secure AI interaction.

</div>

## Connecting AI Clients

The MCP endpoint is exposed at:

```
https://<your-server>/mcp/<api-key>
```

Where `<api-key>` is configured in the AI Server module settings.

### Native HTTP clients

Clients that support MCP over HTTP/SSE natively (Cursor, Windsurf, Zed, Claude Code, etc.) can connect directly using the URL above — no additional software needed.

### Claude Desktop and stdio-only clients

Claude Desktop only supports stdio transport and cannot connect to HTTP MCP servers directly.
Use **cv4pve-mcp-bridge** to proxy stdio ↔ HTTP.

#### cv4pve-mcp-bridge

A lightweight bridge that reads JSON-RPC from stdin and forwards requests to the cv4pve-admin MCP endpoint via HTTP, returning SSE responses back to stdout.

[Download from GitHub Releases :material-download:](https://github.com/Corsinvest/cv4pve-admin/releases){ .md-button .md-button--primary }
[Documentation :material-book-open-variant:](https://github.com/Corsinvest/cv4pve-admin/blob/main/src/Corsinvest.ProxmoxVE.Admin.McpBridge/README.md){ .md-button }

Pre-built binaries are available for Windows, Linux, and macOS. See the [Documentation :material-book-open-variant:](https://github.com/Corsinvest/cv4pve-admin/blob/main/src/Corsinvest.ProxmoxVE.Admin.McpBridge/README.md) for setup instructions.

!!! tip
    Use the **Bridge** tab in the AI Server module to download the binary for your platform and generate a ready-to-use `claude_desktop_config.json` pre-filled with your server URL.

## Available Tools

### Cluster

| Tool | Description |
|------|-------------|
| `ListClusters` | List all configured clusters with name and description |
| `GetClusterStatus` | Cluster health summary: node count (online/offline), VM count (running/stopped/paused) |

### Virtual Machines

| Tool | Description |
|------|-------------|
| `ListVms` | List VMs and containers with status, resource usage. Supports status/type filters and Minimal/Full detail |
| `ListSnapshots` | List VM snapshots with dates. Full detail includes description and parent. Optional disk size |
| `GetVmConfig` | Full VM/LXC configuration: CPU, memory, disks, network, agent, boot options |
| `ChangeVmState` | Change VM power state: Start, Stop (force), Shutdown (graceful), Reset |
| `CreateVmSnapshot` | Create a VM/LXC snapshot, with optional description and RAM state (QEMU only) |
| `DeleteVmSnapshot` | Delete a VM/LXC snapshot |
| `RollbackVmSnapshot` | Rollback a VM/LXC to a snapshot |
| `MigrateVm` | Migrate VM/LXC to another node, with optional live migration and target storage |
| `BackupVm` | Create a VM/LXC backup via vzdump with optional storage, mode, compression, bandwidth limit |
| `ListVmRrdData` | Historical metrics (CPU, Memory, Disk, Network, Pressure PSI) for one or more VMs |

### Nodes

| Tool | Description |
|------|-------------|
| `ListNodes` | List cluster nodes with CPU, memory, disk usage. Minimal/Full detail level |
| `GetNodeStatus` | Detailed node status: PVE version, kernel, CPU model, memory, swap, load average, root fs |
| `ListReplications` | List cluster replications with schedule, sync status and error info |
| `ListNodeRrdData` | Historical metrics (CPU, Load avg, Memory, Swap, Disk, Network, Pressure PSI) for nodes |
| `ListTasks` | List cluster tasks (active and recent) with status, type, user and duration |

### Storage & Backup

| Tool | Description |
|------|-------------|
| `ListStorage` | List cluster storage with type, status, usage. Minimal/Full detail level |
| `ListPools` | List cluster resource pools with descriptions |
| `ListBackups` | List backups on a specific node and storage, with optional VM ID filter |
| `ListStorageContent` | List storage content (ISO, templates, images, backups) on a specific node and storage. Optional content type filter |
| `ListBackupJobs` | List cluster backup jobs with schedule, storage, compression and VM list |

### Query <span class="ee"></span>

| Tool | Description |
|------|-------------|
| `GetQuerySchema` | Get schema (fields and types) for all available query tables |
| `ExecuteQuery` | Execute queries on Proxmox VE cluster data across guests, nodes, storages, snapshots and more |

## Output Format

The **Output Format** setting controls how tool responses are serialized before being sent to the AI model. Choosing the right format reduces token consumption and improves response quality.

The table below shows the same request returned in each format, with approximate token counts as an example — actual values vary depending on dataset size.

| Format | Description | Approx. tokens* |
|--------|-------------|-----------------|
| **JsonCompact** | JSON with `headers` + `rows` arrays | ~450 |
| **JsonNormal** | Standard JSON array of objects | ~600 |
| **Toon** | Token-Oriented Object Notation (`data[23]{...}`) | ~320 |
| **Csv** | Plain CSV with header row | ~250 |

*Approximate values for the same response payload — actual token count depends on dataset size.

The default is **`JsonCompact`**, which offers a good balance between token efficiency and compatibility with all AI models.

!!! tip "Choosing the right format"
    - Use **Toon** or **Csv** to minimize token usage with large datasets.
    - Use **JsonNormal** if your AI client has trouble parsing compact formats.
    - Most AI models work well with the default **JsonCompact**.
