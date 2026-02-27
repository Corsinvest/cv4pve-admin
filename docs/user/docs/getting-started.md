# Installation Guide

!!! tip "Pre-release versions (RC, Beta, ...)"
    Pre-release versions such as `rc`, `beta`, or `alpha` (e.g. `2.0.0-rc1`) **do not require a subscription** — all EE features are unlocked for testing purposes.
    Once you upgrade to a stable release, a valid subscription will be required to use EE features.

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

The installer will ask which edition (CE or EE) and which version to install. The default is `latest` (stable release) — just press Enter to accept. If you need a specific version (e.g. `2.0.0-rc1`, `2.0.0`), enter it when prompted. The version can also be changed later by editing `CV4PVE_ADMIN_TAG` in `.env`.

=== "Linux / macOS"

    ```bash
    curl -fsSL https://raw.githubusercontent.com/Corsinvest/cv4pve-admin/main/install.sh | bash
    ```

=== "Windows PowerShell"

    ```powershell
    irm https://raw.githubusercontent.com/Corsinvest/cv4pve-admin/main/install.ps1 | iex
    ```


!!! tip "Pre-release versions (RC, Beta, ...)"
    Pre-release versions such as `rc`, `beta`, or `alpha` (e.g. `2.0.0-rc1`) **do not require a subscription** — all EE features are unlocked for testing purposes.
    Once you upgrade to a stable release, a valid subscription will be required to use EE features.

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
3. **You will be prompted to change the default password** — this is required before you can proceed
4. Configure your first Proxmox cluster — the setup dialog opens automatically

### Proxmox VE API Credentials

When connecting a Proxmox cluster, assign the **`PVEAdmin`** role at path `/` to the Proxmox user or API token used by cv4pve-admin.

!!! tip "Automatic API Token Creation"
    cv4pve-admin can create a dedicated Proxmox user and API token for you automatically.
    [:octicons-arrow-right-24: Learn more](configuration/admin-area.md#automatic-api-token-creation)

!!! warning "Insufficient permissions"
    Using a role with only a subset of privileges (e.g. `Sys.Audit` alone) will cause permission errors at runtime.
    Always assign `PVEAdmin` at path `/` to ensure full functionality.
    For advanced least-privilege setups, see [:octicons-arrow-right-24: PVE Permissions](configuration/pve-permissions.md).

!!! info "First Login Password Change"
    On first login with the default `admin@local / Password123!` credentials, the application will automatically prompt you to set a new password before you can continue.

---

## Advanced Docker Setup

For detailed Docker deployment information, see the complete Docker documentation:

📖 **[Docker Deployment Guide](https://github.com/Corsinvest/cv4pve-admin/blob/main/src/docker/README.md)**

The installer also creates an `adminctl` script in the installation directory — run `./adminctl help` to see all available commands.

## Next Steps

After installation, you can:

- **[Configuration](configuration/index.md)** - Customize application settings, security, and logging
- **[HTTPS with Reverse Proxy](https.md)** - Enable HTTPS in production
- **[Modules](modules/index.md)** - Explore available modules and features
- **[Troubleshooting](troubleshooting.md)** - Common issues and solutions
