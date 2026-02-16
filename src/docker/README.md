# CV4PVE Admin - Docker Deployment

Quick reference for managing your cv4pve-admin Docker installation.

📖 **[Full Documentation](https://corsinvest.github.io/cv4pve-admin/)** | 🚀 **[Installation Guide](https://corsinvest.github.io/cv4pve-admin/getting-started/)**

---

## Docker Compose Commands

```bash
docker compose up -d              # Start services
docker compose down               # Stop services
docker compose logs -f            # View logs
docker compose ps                 # Check status
docker compose restart            # Restart services
docker compose pull               # Pull latest images
```

**Start with PgWeb (Database Admin Tool):**
```bash
docker compose --profile admin up -d
```

Access PgWeb at: **http://localhost:8082**

---

## Backup & Restore

### Backup Database

```bash
docker compose exec postgres \
  pg_dump -U postgres cv4pve-admin-db > backup-$(date +%Y%m%d_%H%M%S).sql
```

### Restore Database

```bash
docker compose stop cv4pve-admin
docker compose exec -T postgres \
  psql -U postgres cv4pve-admin-db < backup.sql
docker compose start cv4pve-admin
```

---

## Updates

### Automatic Updates (Watchtower)

Updates are managed automatically via Watchtower and can be triggered from the web UI:
1. Navigate to System → Updates
2. See "Update Available" notification
3. Click "Update Now"
4. Wait ~30 seconds for container restart

### Manual Update

```bash
docker compose pull cv4pve-admin
docker compose up -d cv4pve-admin
```

---

## Testing Pre-Release Versions

To test Release Candidate (RC) or specific versions before they become stable:

### 1. Edit `.env` file

Edit `.env` and change `CV4PVE_ADMIN_TAG` to the desired tag:
```bash
CV4PVE_ADMIN_TAG=2.0.0-rc1
```

Available tags (check [Docker Hub](https://hub.docker.com/r/corsinvest/cv4pve-admin/tags) for the full list):
- `latest` - Stable release (default)
- `2.0.0-rc1`, `2.0.0-rc2`, etc. - Release Candidates
- `2.0.0`, `2.1.0` - Specific versions

**Quick commands to change version:**

Linux/Mac (bash):
```bash
# Replace VERSION with your desired tag (e.g. 2.0.0-rc1, 2.0.0, latest)
VERSION=2.0.0-rc1
sed -i "s/^CV4PVE_ADMIN_TAG=.*/CV4PVE_ADMIN_TAG=$VERSION/" .env
```

Windows (PowerShell):
```powershell
# Replace VERSION with your desired tag (e.g. 2.0.0-rc1, 2.0.0, latest)
$VERSION = "2.0.0-rc1"
(Get-Content .env) -replace '^CV4PVE_ADMIN_TAG=.*', "CV4PVE_ADMIN_TAG=$VERSION" | Set-Content .env
```

### 2. Pull and restart

```bash
docker compose down
docker compose pull
docker compose up -d
```

### 3. Verify version

Check the logs to confirm you're running the correct version:
```bash
docker compose logs cv4pve-admin | grep "Version:"
```

### 4. Rollback to stable

To return to the stable version, change `.env` back to:
```bash
CV4PVE_ADMIN_TAG=latest
```

Then repeat step 2.

**Note:** Watchtower automatic updates are disabled for non-`latest` tags to prevent unwanted version changes during testing.

---

## Custom Configuration

For advanced configuration overrides using `appsettings.extra.json`, see the full documentation:

📖 **[Configuration Guide](https://corsinvest.github.io/cv4pve-admin/configuration/)**

---

## Troubleshooting

### Check Service Status

```bash
docker compose ps
```

### View Logs

**All services:**
```bash
docker compose logs -f
```

**Specific service:**
```bash
docker compose logs -f cv4pve-admin
docker compose logs -f postgres
docker compose logs -f apprise  # EE only
```

### Restart Services

```bash
docker compose restart cv4pve-admin
```

### Reset Everything (⚠️ Deletes all data)

```bash
docker compose down -v
rm -rf ./data/*
docker compose up -d
```

---

## Multiple Instances

You can run multiple instances (e.g., production and test) in separate directories:

```bash
# Production (CE)
mkdir /opt/cv4pve-prod && cd /opt/cv4pve-prod
# Copy docker-compose-ce.yaml to docker-compose.yaml, edit .env (port 8080)
cp docker-compose-ce.yaml docker-compose.yaml
docker compose up -d

# Test (EE)
mkdir /opt/cv4pve-test && cd /opt/cv4pve-test
# Copy docker-compose-ee.yaml to docker-compose.yaml, edit .env (port 8081)
cp docker-compose-ee.yaml docker-compose.yaml
docker compose up -d
```

---

## Security Best Practices

- Change default password in `.env` before first start
- Use strong, random `WATCHTOWER_HTTP_API_TOKEN`
- Use reverse proxy (Nginx/Traefik/Caddy) for HTTPS in production
- Backup database regularly
- Keep containers updated via web UI or manual pull
- Limit access to ports 8080/8082 via firewall
