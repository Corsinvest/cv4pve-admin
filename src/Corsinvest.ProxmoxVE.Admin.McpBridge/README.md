# cv4pve-mcp-bridge

```
   ______                _                      __
  / ____/___  __________(_)___ _   _____  _____/ /_
 / /   / __ \/ ___/ ___/ / __ \ | / / _ \/ ___/ __/
/ /___/ /_/ / /  (__  ) / / / / |/ /  __(__  ) /_
\____/\____/_/  /____/_/_/ /_/|___/\___/____/\__/
```

A stdio↔HTTP bridge that connects MCP clients (e.g. Claude Desktop) to a [cv4pve-admin](https://github.com/Corsinvest/cv4pve-admin) MCP server endpoint.

It reads JSON-RPC messages from **stdin**, forwards them via HTTP POST to the MCP server, and writes the SSE responses back to **stdout**.

## Download

Pre-built binaries are available on the [GitHub Releases](https://github.com/Corsinvest/cv4pve-admin/releases) page:

| File | Platform |
|------|----------|
| `cv4pve-mcp-bridge-win-x64.exe` | Windows x64 |
| `cv4pve-mcp-bridge-linux-x64` | Linux x64 |
| `cv4pve-mcp-bridge-linux-arm64` | Linux ARM64 (Raspberry Pi, etc.) |
| `cv4pve-mcp-bridge-osx-x64` | macOS Intel |
| `cv4pve-mcp-bridge-osx-arm64` | macOS Apple Silicon (M1/M2/M3) |

On Linux/macOS, make the binary executable after download:

```bash
chmod +x cv4pve-mcp-bridge-linux-x64
```

## Usage

```
cv4pve-mcp-bridge [options]

Options:
  -u, --url <url>        MCP server endpoint URL. Env: MCP_URL
  -k, --api-key <key>    API key for authentication. Env: MCP_API_KEY
  -i, --insecure         Disable SSL certificate validation. Env: MCP_INSECURE
```

Configuration can be provided via command-line arguments or environment variables. Arguments take priority over environment variables.

## Configuration

### Environment variables

| Variable | Description |
|----------|-------------|
| `MCP_URL` | MCP server endpoint URL |
| `MCP_API_KEY` | API key for authentication |
| `MCP_INSECURE` | Set to `1` or `true` to disable SSL validation (dev only) |

## Claude Desktop integration

Add the following to your `claude_desktop_config.json`:

### With arguments

```json
{
  "mcpServers": {
    "cv4pve-admin": {
      "command": "/path/to/cv4pve-mcp-bridge-linux-x64",
      "args": [
        "--url", "https://your-cv4pve-admin/mcp",
        "--api-key", "your-app-token"
      ]
    }
  }
}
```

### With environment variables

```json
{
  "mcpServers": {
    "cv4pve-admin": {
      "command": "/path/to/cv4pve-mcp-bridge-linux-x64",
      "env": {
        "MCP_URL": "https://your-cv4pve-admin/mcp",
        "MCP_API_KEY": "your-app-token"
      }
    }
  }
}
```

> The API key is an **App Token** generated in cv4pve-admin under **System → Security → App Tokens**.

## How it works

```
Claude Desktop
     │  stdin (JSON-RPC)
     ▼
cv4pve-mcp-bridge
     │  HTTP POST + X-API-Key
     ▼
cv4pve-admin MCP endpoint
     │  SSE response (data: {...})
     ▼
cv4pve-mcp-bridge
     │  stdout (JSON-RPC)
     ▼
Claude Desktop
```

## License

AGPL-3.0-only — Copyright © Corsinvest Srl
