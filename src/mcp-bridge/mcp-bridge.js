#!/usr/bin/env node

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const https = require('https');
const readline = require('readline');

const MCP_URL = process.env.MCP_URL
const API_KEY = process.env.MCP_API_KEY

if (!MCP_URL || !API_KEY) {
  console.error('❌ Error: Missing environment variables.');

  if (!MCP_URL) { console.error(' - MCP_URL is not set'); }

  if (!API_KEY) { console.error(' - MCP_API_KEY is not set'); }
  process.exit(1);
}

const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout,
  terminal: false
});

let buffer = '';

rl.on('line', (line) => {
  buffer += line;

  // Try to parse and send the message
  try {
    const message = JSON.parse(buffer);
    sendToMcp(message);
    buffer = '';
  } catch (e) {
    // Not complete JSON yet, continue buffering
  }
});

function sendToMcp(message) {
  const messageStr = JSON.stringify(message);

  // Log outgoing message to stderr (appears in Claude Desktop logs)
  console.error('[Bridge] Sending:', messageStr);

  const options = {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Content-Length': Buffer.byteLength(messageStr),
      'X-API-Key': API_KEY,
      'Accept': 'application/json, text/event-stream'
    }
  };

  const req = https.request(MCP_URL, options, (res) => {
    console.error(`[Bridge] Response status: ${res.statusCode}`);
    console.error(`[Bridge] Response headers:`, res.headers);

    let sseBuffer = '';

    res.setEncoding('utf8');
    res.on('data', (chunk) => {
      console.error('[Bridge] Received chunk:', chunk);
      sseBuffer += chunk;

      // Parse SSE format
      const lines = sseBuffer.split('\n');

      for (let i = 0; i < lines.length - 1; i++) {
        const line = lines[i].trim();

        if (line.startsWith('data: ')) {
          const jsonData = line.substring(6);
          try {
            // Parse and immediately output the JSON data
            const parsed = JSON.parse(jsonData);
            console.error('[Bridge] Parsed SSE data:', JSON.stringify(parsed));
            // Send to stdout for Claude Desktop
            process.stdout.write(JSON.stringify(parsed) + '\n');
          } catch (e) {
            console.error('[Bridge] Failed to parse SSE data:', e.message);
          }
        }
      }

      // Keep the last incomplete line in buffer
      sseBuffer = lines[lines.length - 1];
    });

    res.on('end', () => {
      console.error('[Bridge] Response complete');

      // Process any remaining buffered data
      if (sseBuffer.trim().startsWith('data: ')) {
        const jsonData = sseBuffer.trim().substring(6);
        try {
          const parsed = JSON.parse(jsonData);
          console.error('[Bridge] Final SSE data:', JSON.stringify(parsed));
          process.stdout.write(JSON.stringify(parsed) + '\n');
        } catch (e) {
          console.error('[Bridge] Failed to parse final SSE data:', e.message);
        }
      }
    });
  });

  req.on('error', (error) => {
    console.error(`[Bridge] Error: ${error.message}`);
  });

  req.write(messageStr);
  req.end();
}

process.on('SIGINT', () => {
  process.exit(0);
});
