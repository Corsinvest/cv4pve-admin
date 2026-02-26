# Admin Area

System administration, security, monitoring and Proxmox VE cluster configuration.

## Overview

Centralized management for Proxmox VE infrastructure. Connect and manage multiple clusters from a single interface.

<div class="grid cards" markdown>

-   :material-dns:{ .lg .middle } **Proxmox VE Clusters**

    ---

    Configure cluster connections, API credentials, and access settings.

    When adding a cluster, you can specify individual nodes and define their **display order** in the grid by setting a position for each node. This allows you to arrange nodes in a custom order rather than the default alphabetical order.

    [:octicons-arrow-right-24: Web API Access](#web-api-access)

    [:octicons-arrow-right-24: SSH Configuration](#ssh-configuration)

-   :material-security:{ .lg .middle } **Security & Access Control** <span class="ee"></span>

    ---

    Permission system to define roles and control access.

-   :material-account-multiple:{ .lg .middle } **User Management** <span class="ee"></span>

    ---

    Create and manage user accounts with role-based access control.

-   :material-shield-account:{ .lg .middle } **Roles & Permissions** <span class="ee"></span>

    ---

    Define custom roles with specific permissions.

-   :material-history:{ .lg .middle } **Audit Logs** <span class="ee"></span>

    ---

    Track administrative actions and user activities.

-   :material-monitor-eye:{ .lg .middle } **Active Sessions** <span class="ee"></span>

    ---

    View and manage active user sessions.

-   :material-information:{ .lg .middle } **System Information**

    ---

    View system metrics including CPU, memory, disk usage, and service status.

-   :material-calendar-clock:{ .lg .middle } **Background Jobs**

    ---

    Monitor and manage scheduled background tasks.

-   :material-file-document:{ .lg .middle } **System Logs** <span class="ee"></span>

    ---

    Access application logs for troubleshooting.

-   :material-wrench:{ .lg .middle } **Maintenance**

    ---

    Perform system maintenance tasks including database cleanup and cache clearing.

-   :material-cog:{ .lg .middle } **General Settings**

    ---

    Configure global application settings.

    - <span class="ce"></span> SMTP server configuration for email notifications
    - <span class="ee"></span> Appearance customization

-   :material-bell:{ .lg .middle } **Notification Hub**

    ---

    Configure notification channels and alert routing.

    - <span class="ce"></span> 2 channels: SMTP and WebHook
    - <span class="ee"></span> 119+ channels: Telegram, Discord, Slack, Teams, Apprise and more

    [:octicons-arrow-right-24: Configure notifications](notifier.md)

</div>

## Web API Access

Configure the Proxmox VE API credentials used to connect to each cluster.

| Field | Description |
|-------|-------------|
| **Access Type** | `Credential` (username/password) or `API Token` |
| **API Token** | Token in format `user@realm!tokenname=secret` |
| **Timeout (msec)** | HTTP request timeout in milliseconds (default: 1000) |
| **Validate Certificate** | Verify TLS certificate of Proxmox VE nodes |

### Automatic API Token Creation

Instead of manually creating an API token in Proxmox, use the **🔑 button** next to the API Token field to generate one automatically.

A dialog will ask for:

- **Username** — a Proxmox user with sufficient privileges (e.g. `root@pam`)
- **Password** — used only during this operation, never stored
- **Token Name** — identifier for the token (default: `cv4pve-admin`)

The following steps are performed automatically on the Proxmox cluster:

1. Creates a dedicated Proxmox user `cv4pve-admin@pve`
2. Assigns the `PVEAdmin` role on path `/` (with propagation)
3. Creates the API token `cv4pve-admin@pve!{token-name}` with `privsep=0` (inherits PVEAdmin role)
4. Populates the **API Token** field with the generated value (`cv4pve-admin@pve!{token-name}=secret`)
5. Shows a one-time dialog to copy the token — **it will not be shown again**

The username and password are used only during this wizard and are **never saved**.

---

## SSH Configuration

Several features require SSH access to the Proxmox VE nodes, including snapshot size calculation.

SSH is configured per-cluster under **Proxmox VE Clusters → SSH Credentials**.

| Field | Description |
|-------|-------------|
| **Auth Method** | `Password` or `Private Key` |
| **Username** | SSH user (typically `root`) |
| **Password** | Used when Auth Method is Password |
| **Private Key** | PEM/OpenSSH private key content |
| **Passphrase** | Optional passphrase for private key |
| **Timeout (msec)** | Connection timeout in milliseconds (default: 5000) |

!!! tip
    Use **Private Key** authentication for better security. You can upload the key file directly from the UI.

!!! note
    The SSH timeout applies only to the **connection phase** (handshake), not to command execution duration.

Use the **Test SSH** button to verify connectivity to all nodes before saving.

---

## Access

Access the Admin Area from the main menu or profile dropdown in the top navigation bar.
