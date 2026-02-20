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
Use the **MCP Bridge** included in the repository (`src/mcp-bridge/`) to proxy stdio ↔ HTTP.

See the [MCP Bridge README](https://github.com/Corsinvest/cv4pve-admin/blob/main/src/mcp-bridge/README.md) for setup instructions.
