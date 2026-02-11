#!/bin/bash

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

export MCP_URL="https://localhost:7251/mcp/aa"
export MCP_API_KEY="superkey123"

# Run the Node.js bridge
node "$SCRIPT_DIR/mcp-bridge.js"
