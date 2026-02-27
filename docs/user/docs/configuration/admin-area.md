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

### WEB API Access Type — Feature Limitations

The choice of **Access Type** affects which features are available:

| Feature | Credential (PAM user) | Credential (non-PAM) | API Token |
|---------|:---------------------:|:--------------------:|:---------:|
| **SSH — Same as WEB API** | ✅ | ❌ PAM only | ❌ |
| **Node console** (NoVnc, Xterm.js) | ✅ | ❌ PAM only | ❌ |
| **VM/LXC console** (NoVnc, Xterm.js, Spice) | ✅ | ✅ | ❌ |

!!! info "Why PAM for node console and SSH Same as WEB API?"
    Node consoles (NoVnc, Xterm.js) and SSH require OS-level credentials — i.e. a Linux system user.
    Only PAM users (`@pam` realm, or username without realm) map to actual Linux users on the Proxmox nodes.
    Users from other realms (PVE, LDAP, etc.) exist only in Proxmox's internal database and have no corresponding SSH/OS account.

!!! info "Why not API Token for console?"
    Proxmox VE requires an active **user session** (ticket) to open a VM or node console via NoVnc/Xterm.js/Spice.
    API Tokens do not create a session ticket, so console access is not possible when using API Token authentication.

---

## SSH Configuration

Several features require SSH access to the Proxmox VE nodes, including snapshot size calculation.

SSH is configured per-cluster under **Proxmox VE Clusters → SSH Credentials**.

| Field | Description |
|-------|-------------|
| **Auth Method** | `None`, `Password`, `Private Key`, or `Same as WEB API` |
| **Username** | SSH user (typically `root`) |
| **Password** | Used when Auth Method is `Password` |
| **Private Key** | PEM/OpenSSH private key content |
| **Passphrase** | Optional passphrase for private key |
| **Timeout (msec)** | Connection timeout in milliseconds (default: 5000) |

### Auth Methods

| Method | Description |
|--------|-------------|
| **None** | SSH disabled — features requiring SSH will be skipped |
| **Password** | Authenticate with username and password |
| **Private Key** | Authenticate with a private key (recommended) |
| **Same as WEB API** | Reuse WEB API credentials (username without `@realm`, same password) |

!!! warning "Same as WEB API — PAM users only"
    The `Same as WEB API` method is only valid for **PAM users** (e.g. `root@pam`).
    Users authenticated via other realms (PVE, LDAP, etc.) do not have corresponding SSH credentials.
    If the WEB API user is not a PAM user, a warning will be shown and SSH will not work.

!!! tip
    Use **Private Key** authentication for better security. You can upload the key file directly from the UI.

!!! note
    The SSH timeout applies only to the **connection phase** (handshake), not to command execution duration.

Use the **Test SSH** button to verify connectivity to all nodes before saving.

---

## Access

Access the Admin Area from the main menu or profile dropdown in the top navigation bar.
