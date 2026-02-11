@echo off

set MCP_URL=https://localhost:7251/mcp/aa
set MCP_API_KEY=superkey123

node "%~dp0mcp-bridge.js"
