# CV4PVE Admin - Docker Deployment

Quick reference for Docker Compose deployment with support for both Community Edition (CE) and Enterprise Edition (EE).

## Quick Install

**Linux/Mac:**
```bash
curl -fsSL https://raw.githubusercontent.com/Corsinvest/cv4pve-admin/main/install.sh | bash
```

**Windows PowerShell:**
```powershell
irm https://raw.githubusercontent.com/Corsinvest/cv4pve-admin/main/install.ps1 | iex
```

This creates a `cv4pve-admin-docker` directory with all required files. Continue with [Quick Start](#quick-start) below.

---

## Quick Start

### 1. Choose Edition

Two docker-compose files are available:
- **`docker-compose-ce.yaml`** - Community Edition
- **`docker-compose-ee.yaml`** - Enterprise Edition

**Copy or symlink the one you want to `docker-compose.yaml`:**

```bash
# Community Edition
cp docker-compose-ce.yaml docker-compose.yaml

# OR Enterprise Edition
cp docker-compose-ee.yaml docker-compose.yaml

# OR use symlink (Linux/Mac)
ln -s docker-compose-ce.yaml docker-compose.yaml
```

### 2. Configure

Edit `.env` and customize the following key settings:

```bash
# IMPORTANT: Change this before production!
POSTGRES_PASSWORD=your-secure-password

# Optional: adjust port
CV4PVE_ADMIN_PORT=8080
```

The `.env` file includes all available configuration options with detailed comments:
- PostgreSQL database settings
- Network ports (application, PgWeb)
- Data and backup directories
- Docker image version/tag
- Timezone configuration
- Watchtower API token

### 3. Start

```bash
docker compose up -d
```

### 4. Access

**http://localhost:8080**

Default credentials: `admin@local` / `Password123!` (change after first login)

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

Change the version tag:
```bash
CV4PVE_ADMIN_TAG=rc2
```

Available tags:
- `latest` - Stable release (default)
- `rc2`, `rc3`, etc. - Release Candidates
- `1.0.0`, `1.1.0` - Specific versions

**Quick commands to change version:**

Linux/Mac (bash):
```bash
# Replace VERSION with your desired tag (rc2, rc3, 1.0.0, latest, etc.)
VERSION=rc2
sed -i "s/^CV4PVE_ADMIN_TAG=.*/CV4PVE_ADMIN_TAG=$VERSION/" .env
```

Windows (PowerShell):
```powershell
# Replace VERSION with your desired tag (rc2, rc3, 1.0.0, latest, etc.)
$VERSION = "rc2"
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

For advanced configuration overrides without modifying environment variables, you can use `appsettings.extra.json`.

### Location

The file is automatically created at: `${DATA_DIR}/cv4pve-admin/config/appsettings.extra.json`

### How to Use

1. **Edit the file:**
```bash
nano ${DATA_DIR}/cv4pve-admin/config/appsettings.extra.json
```

2. **Add your overrides** (example):
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}
```

3. **Changes apply automatically** - No restart needed! (thanks to `reloadOnChange`)

### Common Use Cases

**Change log level:**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft.EntityFrameworkCore.Database.Command": "Information"
      }
    }
  }
}
```

**Configure reverse proxy:**
```json
{
  "PathBase": "/admin",
  "ForwardedHeaders": {
    "Enabled": true
  }
}
```

**Adjust timeouts:**
```json
{
  "HttpClient": {
    "Timeout": 300000
  }
}
```

**Note:** This file only contains your customizations. The main `appsettings.json` remains managed by the application.

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
