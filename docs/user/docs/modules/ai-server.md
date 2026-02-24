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
