# CV4PVE Admin - Docker Deployment

Quick reference for managing your cv4pve-admin Docker installation.

📖 **[Full Documentation](https://corsinvest.github.io/cv4pve-admin/)** | 🚀 **[Installation Guide](https://corsinvest.github.io/cv4pve-admin/getting-started/)**

---

## adminctl

`adminctl` is a management helper script included in the installation directory.

```bash
./adminctl help
```

---

## Updates

When a new version is available, a badge **🆕 vX.Y.Z** appears in the top navigation bar.
Click it to trigger an immediate update via Watchtower (requires `WATCHTOWER_HTTP_API_TOKEN` in `.env`).

The application checks for new versions every 12 hours.

### Refresh `adminctl` and the compose files

`adminctl` itself and the `docker-compose-{ce,ee}.yaml` files can fall behind over time.
To pull the latest copy from GitHub `main`:

```bash
./adminctl self-update
```

This:

- Downloads the latest `adminctl` + `docker-compose-{ce,ee}.yaml`
- Snapshots the current files into `self-update-backups/<timestamp>/` so a rollback is always possible
- Regenerates the active `docker-compose.yaml` to match the detected edition (CE or EE)
- Leaves `.env`, `data/`, and `backups/` untouched

If your `docker-compose.yaml` is customized (differs from both `docker-compose-ce.yaml` and `docker-compose-ee.yaml`), it is **not** rewritten — you will see a warning and can merge the changes manually by comparing with the updated `docker-compose-{ce,ee}.yaml`.

After running, restart the stack to apply changes:

```bash
./adminctl restart
# or, to pull new container images at the same time:
docker compose up -d
```

---

## Testing Pre-Release Versions

To test RC or specific versions, edit `CV4PVE_ADMIN_TAG` in `.env` then restart.

📖 **[Testing Pre-Release Versions](https://corsinvest.github.io/cv4pve-admin/getting-started/#change-version-after-installation)**

---

## Custom Configuration

For advanced configuration overrides using `appsettings.extra.json`, see the full documentation:

📖 **[Configuration Guide](https://corsinvest.github.io/cv4pve-admin/configuration/)**

---

## Security Best Practices

- Change default password in `.env` before first start
- Use strong, random `WATCHTOWER_HTTP_API_TOKEN`
- Use reverse proxy (Nginx/Traefik/Caddy) for HTTPS in production
- Backup database regularly
- Keep containers updated via web UI or manual pull
- Limit access to ports 8080/8082 via firewall
