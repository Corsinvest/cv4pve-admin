---
hide:
  - navigation
  #- toc
---

# Installation Guide

Get cv4pve-admin up and running in minutes with Docker.

---

## Prerequisites

Before installing cv4pve-admin, ensure you have:

- **Docker** 24.0 or later ([Install Docker](https://docs.docker.com/get-docker/))
- **Docker Compose** 2.20 or later (included with Docker Desktop)
- **Minimum 2GB RAM** available for containers
- **10GB disk space** for application and database
- **Proxmox VE** 6.2 or later to manage

!!! tip "System Requirements"
    For production use, we recommend at least 4GB RAM and 20GB disk space.

---

## Quick Installation

The fastest way to get started is using our automated installer:

=== "Linux / macOS"

    ```bash
    curl -fsSL https://raw.githubusercontent.com/Corsinvest/cv4pve-admin/master/install.sh | bash
    ```

    The installer will:

    1. Welcome you and explain the process
    2. Ask which edition you want (CE or EE)
    3. Ask which version/tag to use (default: `latest`)
    4. Request PostgreSQL password (minimum 8 characters)
    5. Download all necessary files from GitHub
    6. Configure environment variables
    7. Start all Docker containers
    8. Display access information

=== "Windows PowerShell"

    ```powershell
    irm https://raw.githubusercontent.com/Corsinvest/cv4pve-admin/master/install.ps1 | iex
    ```

    The installer will:

    1. Welcome you and explain the process
    2. Ask which edition you want (CE or EE)
    3. Ask which version/tag to use (default: `latest`)
    4. Request PostgreSQL password (minimum 8 characters)
    5. Download all necessary files from GitHub
    6. Configure environment variables
    7. Start all Docker containers
    8. Display access information

!!! success "Installation Complete"
    After installation completes, open your browser to **http://localhost:8080**

---

## First Time Setup

After installation, access the application:

1. **Open your browser** to `http://localhost:8080`
2. **Login** with default credentials:
   - Username: `admin@local`
   - Password: `Password123!`
3. **Configure your first Proxmox cluster**:
   - After login, the system automatically opens the cluster configuration dialog
   - Enter your Proxmox VE details:
     - **Host**: Proxmox VE hostname or IP address
     - **Username**: API user (e.g., `root@pam` or token)
     - **Password/Token**: Credentials for authentication
     - **Node**: Proxmox node name
4. **Change the default password**:
   - Navigate to your Profile settings
   - Update the default password immediately

!!! danger "Security Alert"
    Change the default `admin@local/Password123!` credentials immediately after setup!

---

## Configuration Options

### Custom Configuration

You can override application settings by creating an `appsettings.extra.json` file:

```bash
# Create the file in your installation directory
touch appsettings.extra.json
```

Edit the file with your custom settings:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  },
  "ApplicationOptions": {
    "SomeSetting": "value"
  }
}
```

The file is automatically mounted read-only into the container and hot-reloaded when changed.

### Environment Variables

All settings in `.env` can be customized:

| Variable | Default | Description |
|----------|---------|-------------|
| `POSTGRES_PASSWORD` | `cv4pve-admin` | PostgreSQL database password |
| `CV4PVE_ADMIN_TAG` | `latest` | Docker image tag/version |
| `POSTGRES_PORT` | `5432` | PostgreSQL port (internal) |
| `CV4PVE_ADMIN_PORT` | `8080` | Application web port |
| `PGWEB_PORT` | `8082` | PgWeb admin interface port |

### Port Configuration

To change the application port, edit `.env`:

```bash
CV4PVE_ADMIN_PORT=8080
```

Then restart services:

```bash
docker compose restart
```

---

## Testing Pre-Release Versions

To test release candidates or specific versions:

=== "Linux / macOS"

    ```bash
    # Edit .env file
    sed -i '' 's/CV4PVE_ADMIN_TAG=.*/CV4PVE_ADMIN_TAG=rc2/' .env

    # Or manually edit the file
    nano .env

    # Restart containers
    docker compose down && docker compose up -d
    ```

=== "Windows PowerShell"

    ```powershell
    # Edit .env file
    (Get-Content .env) -replace 'CV4PVE_ADMIN_TAG=.*', 'CV4PVE_ADMIN_TAG=rc2' | Set-Content .env

    # Or manually edit the file
    notepad .env

    # Restart containers
    docker compose down; docker compose up -d
    ```

Available tags:

- `latest` - Latest stable release
- `rc1`, `rc2`, etc. - Release candidates
- `beta1`, `beta2`, etc. - Beta release
- `v1.0.0`, `v1.1.0`, etc. - Specific versions

---

## Management Commands

The `adminctl` script provides convenient management commands:

```bash
# View logs
./adminctl logs

# Restart services
./adminctl restart

# Stop services
./adminctl stop

# Start services
./adminctl start

# Open PgWeb database interface
./adminctl pgweb

# Backup database
./adminctl backup

# View status
./adminctl status
```

!!! info "PgWeb Access"
    PgWeb provides a web interface for PostgreSQL at `http://localhost:8082`

---

## Troubleshooting

### Containers Won't Start

Check logs for errors:

```bash
docker compose logs cv4pve-admin
```

Common issues:

- **Port already in use**: Change `CV4PVE_ADMIN_PORT` in `.env`
- **Permission denied**: Ensure user has Docker permissions
- **Out of memory**: Increase Docker memory limit

### Database Connection Failed

Verify PostgreSQL is running:

```bash
docker compose ps postgres
docker compose logs postgres
```

Check password matches in `.env` file.

### Application Won't Load

1. Check all containers are healthy:
   ```bash
   docker compose ps
   ```

2. Verify port forwarding:
   ```bash
   curl http://localhost:5000
   ```

3. Check application logs:
   ```bash
   docker compose logs cv4pve-admin -f
   ```

### Reset Everything

To completely reset and start fresh:

```bash
# Stop and remove all containers
docker compose down -v

# Remove images (optional)
docker compose down --rmi all

# Start fresh
docker compose up -d
```

!!! warning "Data Loss"
    Using `-v` flag will delete all data including database!

---

## Updating

cv4pve-admin includes automatic updates via Watchtower, but you can also update manually:

### Automatic Updates (Watchtower)

Watchtower automatically checks for new images and updates containers. It runs daily by default.

To disable automatic updates, remove the `watchtower` service from `docker-compose.yaml`.

### Manual Update

```bash
# Pull latest images
docker compose pull

# Restart containers with new images
docker compose up -d

# Clean up old images
docker image prune -f
```

### Update to Specific Version

```bash
# Edit .env to change version
sed -i 's/CV4PVE_ADMIN_TAG=.*/CV4PVE_ADMIN_TAG=v1.2.0/' .env

# Pull and restart
docker compose pull && docker compose up -d
```

---

## Multiple Instances

To run multiple instances (e.g., production and testing):

1. Create separate directories:
   ```bash
   mkdir cv4pve-admin-prod
   mkdir cv4pve-admin-test
   ```

2. Copy configuration to each:
   ```bash
   cp -r cv4pve-admin/src/docker/* cv4pve-admin-prod/
   cp -r cv4pve-admin/src/docker/* cv4pve-admin-test/
   ```

3. Edit `.env` in each directory with different ports:
   ```bash
   # Production
   CV4PVE_ADMIN_PORT=5000

   # Testing
   CV4PVE_ADMIN_PORT=5001
   ```

4. Start each instance from its directory:
   ```bash
   cd cv4pve-admin-prod && docker compose up -d
   cd ../cv4pve-admin-test && docker compose up -d
   ```

---

## Uninstallation

To completely remove cv4pve-admin:

```bash
# Stop and remove containers
docker compose down

# Remove volumes (all data will be lost!)
docker compose down -v

# Remove images
docker rmi corsinvest/cv4pve-admin:latest
docker rmi postgres:17-alpine

# Remove files
cd .. && rm -rf cv4pve-admin
```

---

## Next Steps

After successful installation:

1. **Secure your installation** - Change default passwords
2. **Add Proxmox clusters** - Configure your infrastructure
3. **Set up automated snapshots** - Configure backup policies
4. **Explore the dashboard** - Familiarize yourself with features
5. **Read the User Guide** - Learn advanced features

[**Continue to User Guide →**](user_guide.md){ .md-button .md-button--primary }

---

## Getting Help

If you encounter issues:

- **Community Support**: [GitHub Issues](https://github.com/Corsinvest/cv4pve-admin/issues)
- **Documentation**: [User Guide](user_guide.md)
- **Enterprise Support**: support@corsinvest.it (EE only)
