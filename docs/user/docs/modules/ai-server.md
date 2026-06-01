# :material-hub: AI Server <span class="scope" data-scope="all-clusters"></span>

Model Context Protocol (MCP) endpoint that lets AI assistants query the cluster, manage VMs and automate operations through natural language.

## Features

<div class="grid cards" markdown>

- :material-robot:{ .lg .middle } **AI Assistant Integration**

    ---

    Manage Proxmox infrastructure through natural language via AI assistants.

- :material-database-eye:{ .lg .middle } **Data Access**

    ---

    AI tools can query VM status, cluster health, and resource usage.

- :material-auto-fix:{ .lg .middle } **Automated Tasks**

    ---

    AI assistants can perform VM management, snapshot creation, and monitoring.

- :material-api:{ .lg .middle } **MCP Protocol**

    ---

    Uses Model Context Protocol for AI integration.

- :material-link-variant:{ .lg .middle } **AI Tool Connection**

    ---

    Connect AI assistants to Proxmox environments.

- :material-shield-lock:{ .lg .middle } **Access Control**

    ---

    Controlled API exposure for secure AI interaction.

</div>

## Why

Why an MCP server when the PVE API is already there?

<div class="why-grid" markdown>

<div markdown>
!!! tip "Talk to the cluster in English"
    Ask "which VMs use the most memory on pve01?" instead of writing the API call. Lower the barrier for everyone on the team.
</div>

<div markdown>
!!! success "Cluster context already loaded"
    The MCP server knows about every cluster you configured — the AI doesn't have to discover, authenticate or paginate the PVE API itself.
</div>

<div markdown>
!!! info "Multi-cluster from one connection"
    One AI assistant sees and operates on every cluster in cv4pve-admin — without juggling tokens or hostnames per cluster.
</div>

<div markdown>
!!! warning "Permission-aware"
    Tools respect the user's PVE permissions — the assistant can't do what the underlying account can't do. Write tools require explicit role grants.
</div>

</div>

## Sections

- **API Access** — generate and rotate the API key used by AI clients to authenticate against the MCP endpoint
- **Bridge** — download the platform-specific `cv4pve-mcp-bridge` binary and a ready-to-use `claude_desktop_config.json` for stdio-only clients (Claude Desktop)

## Connecting AI Clients

### First-time setup

Before connecting any AI client, generate the API key:

1. Open the **AI Server** module and go to the **API Access** tab
2. Click **Regenerate** on the AI Server token
3. Copy the key — it will be used in the connection URL below

!!! warning "Copy the key now"
    The API key is shown only once after generation. Copy it before closing the dialog.

### MCP endpoint URL

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

!!! tip "Shortcut from the UI"
    Use the **Bridge** tab in the AI Server module to download the binary for your platform and generate a ready-to-use `claude_desktop_config.json` pre-filled with your server URL.

## Available Tools

33 tools grouped by area, plus 2 additional Enterprise tools for SQL-like queries.

??? note tools "Show all tools"

    ### Cluster

    | Tool | Description |
    |------|-------------|
    | `ListClusters` | List all configured clusters with name and description |
    | `GetClusterStatus` | Cluster health summary: node count (online/offline), VM count (running/stopped/paused), storage count |
    | `GetClusterOptions` | Cluster options: migration type, network, bandwidth limit, HA settings, console type |

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
    | `ListIsos` | List ISO images available on a node storage |
    | `ListTemplates` | List CT templates available on a node storage |
    | `ListBackupJobs` | List cluster backup jobs with schedule, storage, compression and VM list |
    | `DownloadIso` | Download an ISO image to a node storage from a URL |
    | `DeleteBackup` | Delete a backup from storage (use `ListBackups` to get the volid) |
    | `DeleteIso` | Delete an ISO image from storage (use `ListIsos` to get the volid) |
    | `DeleteStorageContent` | Delete content from storage — ISO / template / image / backup (use `ListStorageContent` to get the volid) |

    ### <span class="ee"></span> Query

    | Tool | Description |
    |------|-------------|
    | `GetQuerySchema` | Get detailed schema (fields and types) for Proxmox VE query tables |
    | `ExecuteQuery` | Execute SQL-like queries on cluster data (guests, nodes, storages, snapshots, …) with filters, grouping, aggregates (COUNT/SUM/AVERAGE/MIN/MAX) and ordering |

    The query JSON shape is:

    ```json
    {
      "from": "table",
      "select": ["fields", "COUNT(field) as alias"],
      "where": {
        "logic": "and|or",
        "conditions": [{ "field": "X", "operator": "=", "value": ["Y"] }]
      },
      "groupBy": ["field"],
      "orderBy": [{ "field": "X", "direction": "asc|desc" }],
      "limit": 10,
      "offset": 0
    }
    ```

    Operators: `=`, `!=`, `>`, `<`, `>=`, `<=`, `CONTAINS`, `IN`, `NOT IN`, `BETWEEN`, `STARTSWITH`, `ENDSWITH`.

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

## Example Prompts

Real prompts you can paste into your AI assistant once the MCP endpoint is connected. The assistant picks the right tool(s) and chains them automatically.

??? note examples "Read-only inspection"

    > Which VMs in cluster `home-lab` are currently running and using more than 70% CPU?

    > Show me the snapshots older than 30 days across all clusters.

    > Summarise the health of cluster `prod`: nodes online/offline, VM status counts, free storage per pool.

    > List the last 20 failed tasks across the cluster, with user and duration.

    > For node `pve01`, show the CPU and memory pressure (PSI) over the last 24 hours.

??? note examples "Backup / storage management"

    > List all backups for VM 101 and tell me the most recent one.

    > On node `pve01`, storage `local`, find ISO images older than 6 months and propose which to delete.

    > Download the latest Debian 12 netinst ISO to storage `local` on node `pve01`.

    > Show me all backup jobs and highlight any whose last run failed.

??? note examples "Operations (write actions)"

    > Create a snapshot of VM 200 called `pre-upgrade` with description "before kernel update", including RAM.

    > Migrate VM 105 from `pve01` to `pve02`, live migration if running.

    > Run a backup of VM 200 on storage `pbs-main` with zstd compression.

    > Reset VM 150 (it's hung).

??? note examples "<span class='ee'></span> Advanced queries"

    Enterprise adds the `GetQuerySchema` + `ExecuteQuery` pair. Useful prompts:

    > What tables can I query against this cluster?

    > Group all guests by node and count how many are running vs stopped.

    > Find the top 5 VMs by memory usage, with their node and OS type.

    > List storages with more than 80% usage.

!!! warning "Permissions matter"
    Write tools (`ChangeVmState`, `CreateVmSnapshot`, `DeleteVmSnapshot`, `RollbackVmSnapshot`, `MigrateVm`, `BackupVm`, `Delete*`, `DownloadIso`) require both the tool permission **and** the underlying Proxmox VE permission on the target resource. The default role grants read-only access; grant write permissions explicitly in **AI Server → Roles**.

## Settings

??? note settings "Show all settings"

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **Enabled** | off | Master on/off switch for the MCP endpoint |
    | **Output Format** | JsonCompact | How tool responses are serialised before being sent to the AI model: `JsonCompact` · `JsonNormal` · `Toon` · `Csv` |
