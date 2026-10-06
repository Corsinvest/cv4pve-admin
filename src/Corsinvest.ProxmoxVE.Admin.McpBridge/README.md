# cv4pve-mcp-bridge

```
   ______                _                      __
  / ____/___  __________(_)___ _   _____  _____/ /_
 / /   / __ \/ ___/ ___/ / __ \ | / / _ \/ ___/ __/
/ /___/ /_/ / /  (__  ) / / / / |/ /  __(__  ) /_
\____/\____/_/  /____/_/_/ /_/|___/\___/____/\__/
```

A stdio↔HTTP bridge that connects MCP clients (e.g. Claude Desktop) to a [cv4pve-admin](https://github.com/Corsinvest/cv4pve-admin) MCP server endpoint.

It reads JSON-RPC messages from **stdin**, forwards them to the MCP server using the MCP Streamable HTTP transport, and writes the responses back to **stdout**.

## Documentation

Download, options, the Claude Desktop configuration and how the bridge works are in the cv4pve-admin documentation:

**[AI Server: MCP Bridge](https://corsinvest.github.io/cv4pve-admin/modules/ai-server-bridge/)**

Pre-built binaries for Windows, Linux and macOS are on the [GitHub Releases](https://github.com/Corsinvest/cv4pve-admin/releases) page.

## License

AGPL-3.0-only. Copyright © Corsinvest Srl
