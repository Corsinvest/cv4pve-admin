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
