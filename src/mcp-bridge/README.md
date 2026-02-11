# MCP Bridge for cv4pve-admin

This directory contains a Node.js bridge that connects Claude Desktop to the cv4pve-admin MCP server.

## What it does

Claude Desktop only supports stdio transport for MCP servers. This bridge converts stdio communication to HTTP requests to the cv4pve-admin MCP server.

## Files

- **mcp-bridge.js**: Node.js script that handles the stdio-to-HTTP conversion
- **mcp-bridge.bat**: Windows batch file to launch the bridge
- **mcp-bridge.sh**: Linux/macOS shell script to launch the bridge

## Configuration

### 1. Configure MCP Server Connection

Edit the appropriate script for your platform:

**Windows (`mcp-bridge.bat`):**
```batch
set MCP_URL=https://localhost:7251/mcp/aa
set MCP_API_KEY=superkey123
```

**Linux/macOS (`mcp-bridge.sh`):**
```bash
export MCP_URL="https://localhost:7251/mcp/aa"
export MCP_API_KEY="superkey123"
```

**Important:**
- Change `MCP_URL` to your cv4pve-admin server URL
- Change `MCP_API_KEY` to match your cv4pve-admin API key

### 2. Configure Claude Desktop

#### Windows

Add this to your `claude_desktop_config.json` (usually at `%APPDATA%\Claude\claude_desktop_config.json`):

```json
{
  "mcpServers": {
    "cv4pve-admin": {
      "command": "cmd",
      "args": [
        "/c",
        "C:\\path\\to\\cv4pve-admin\\ce\\src\\mcp-bridge\\mcp-bridge.bat"
      ]
    }
  }
}
```

#### Linux/macOS

Add this to your `claude_desktop_config.json` (usually at `~/.config/Claude/claude_desktop_config.json`):

```json
{
  "mcpServers": {
    "cv4pve-admin": {
      "command": "/path/to/cv4pve-admin/ce/src/mcp-bridge/mcp-bridge.sh"
    }
  }
}
```

Make sure to make the script executable:
```bash
chmod +x /path/to/cv4pve-admin/ce/src/mcp-bridge/mcp-bridge.sh
```

### 3. Configure Claude Code (CLI)

For Claude Code CLI, create or edit `.claude/mcp.json` in your project directory:

**Windows:**
```json
{
  "mcpServers": {
    "cv4pve-admin": {
      "command": "cmd",
      "args": [
        "/c",
        "C:\\path\\to\\cv4pve-admin\\ce\\src\\mcp-bridge\\mcp-bridge.bat"
      ]
    }
  }
}
```

**Linux/macOS:**
```json
{
  "mcpServers": {
    "cv4pve-admin": {
      "command": "/path/to/cv4pve-admin/ce/src/mcp-bridge/mcp-bridge.sh"
    }
  }
}
```

Then restart Claude Code or the MCP tools will be automatically available.

## Requirements

- Node.js must be installed and available in PATH
- cv4pve-admin server must be running and accessible

## Testing

To test the bridge manually, run the appropriate script:

**Windows:**
```cmd
cd C:\path\to\cv4pve-admin\ce\src\mcp-bridge
mcp-bridge.bat
```

**Linux/macOS:**
```bash
cd /path/to/cv4pve-admin/ce/src/mcp-bridge
./mcp-bridge.sh
```

The bridge will wait for JSON-RPC messages on stdin. You should see connection logs in stderr.

## Security Note

This bridge disables SSL certificate verification (`NODE_TLS_REJECT_UNAUTHORIZED=0`) to work with self-signed certificates during development. For production use, consider using a valid SSL certificate.

## Troubleshooting

### Connection Issues

- Verify cv4pve-admin is running and accessible at the configured `MCP_URL`
- Check that the `MCP_API_KEY` matches your server configuration
- Look for error messages in Claude Desktop logs (Help → View Logs)

### Node.js Not Found

- Ensure Node.js is installed: https://nodejs.org
- Verify Node.js is in your system PATH by running: `node --version`

### Permission Denied (Linux/macOS)

- Make the shell script executable: `chmod +x mcp-bridge.sh`
