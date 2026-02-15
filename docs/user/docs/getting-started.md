# Installation Guide

!!! warning "v2 is not compatible with v1"
    cv4pve-admin v2 is a **complete rewrite** and is not compatible with v1.
    There is currently no migration path from v1 to v2 — a fresh installation is required.

Get cv4pve-admin up and running in minutes with Docker.

---

## Prerequisites

Before installing cv4pve-admin, ensure you have:

- **Docker** 24.0 or later ([Install Docker](https://docs.docker.com/get-docker/))
- **Docker Compose** 2.20 or later (included with Docker Desktop)
- **Proxmox VE** 6.2 or later to manage

---

## Quick Installation

The installer will ask which edition (CE or EE) and which version to install. The default is `latest` (stable release) — just press Enter to accept. If you need a specific version (e.g. `rc1`, `v2.0.0`), enter it when prompted. The version can also be changed later by editing `CV4PVE_ADMIN_TAG` in `.env`.

=== "Linux / macOS"

    ```bash
    curl -fsSL https://raw.githubusercontent.com/Corsinvest/cv4pve-admin/main/install.sh | bash
    ```

=== "Windows PowerShell"

    ```powershell
    irm https://raw.githubusercontent.com/Corsinvest/cv4pve-admin/main/install.ps1 | iex
    ```

!!! success "Installation Complete"
    After installation completes, open your browser to **http://localhost:8080**

### Change version after installation

Edit `CV4PVE_ADMIN_TAG` in `.env` (e.g. `nano .env` or `notepad .env`), then restart:

```bash
docker compose down && docker compose up -d
```

---

## First Time Setup

1. Open **http://localhost:8080**
2. Login: `admin@local` / `Password123!`
3. Configure your first Proxmox cluster — the setup dialog opens automatically
4. **Change the default password** from Profile settings

!!! danger "Security Alert"
    Change the default `admin@local/Password123!` credentials immediately after setup!

---

## Advanced Docker Setup

For detailed Docker deployment information, see the complete Docker documentation:

📖 **[Docker Deployment Guide](https://github.com/Corsinvest/cv4pve-admin/blob/main/src/docker/README.md)**

The installer also creates an `adminctl` script in the installation directory for convenient management (logs, start, stop, restart, backup, status).

Includes:
- Docker Compose commands reference
- Backup and restore procedures
- Update management (automatic and manual)
- Testing pre-release versions (RC)
- Custom configuration with `appsettings.extra.json`
- Troubleshooting and common issues
- Running multiple instances
- Security best practices

## Next Steps

After installation, you can:

- **[Configuration](configuration/index.md)** - Customize application settings, security, and logging
- **[Modules](modules/index.md)** - Explore available modules and features
- **[Troubleshooting](troubleshooting.md)** - Common issues and solutions
