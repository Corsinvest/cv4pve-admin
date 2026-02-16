# Troubleshooting

## Reset a Forgotten Password

If you are locked out of the web UI, reset the admin password via CLI:

```bash
docker compose run --rm cv4pve-admin user reset-password -u admin@local -p NewPassword123!
```

For all available CLI commands, see [CLI Reference](cli.md).

## HTTPS / Reverse Proxy

To expose cv4pve-admin over HTTPS, use a reverse proxy (Caddy, Nginx Proxy Manager, Traefik, or Nginx).

📖 **[HTTPS Setup Guide](https.md)**

