# <span class="ee"></span> :material-security: Security & Access Control

Fine-grained permission system that controls who can use which feature on which cluster.

It runs on top of (and is independent from) the Proxmox VE permission model: PVE permissions decide what the app can do *against* the Proxmox API, while these in-app permissions decide what each user can do *inside* the app.

## Sections

- **Users** — create / edit / disable accounts, assign roles, force 2FA
- **Roles** — named bundles of permissions, built-in or custom
- **Permissions** — per-module, per-cluster, with three scopes (Application, Cluster, All clusters)
- **App Tokens** — machine-to-machine credentials for the REST API
- **Active Sessions** — view and revoke active browser sessions
- **Audit Logs** — full audit trail of administrative actions

## Users

Create, edit and disable user accounts under **Security → Users**.

| Field | Description |
|-------|-------------|
| **Email / Username** | Login identifier. Email is also used for password reset and notifications. |
| **First / Last name** | Display name in the UI and audit log. |
| **Roles** | One or more roles. The user inherits the union of all permissions granted by the assigned roles. |
| **Enabled** | Disabled users cannot log in but their audit history is preserved. |
| **2FA Required** | Force the user to set up two-factor authentication on next login. |

When a new user is created from the UI they receive an **activation email** with a password-reset link, instead of being created with a default password.

!!! tip "First admin user"
    On first startup the app creates `admin@local` with a known default password (`Password123!`). The first login forces a password change. See [Getting Started](../../getting-started.md).

## Roles

A role is a named bundle of permissions. Create roles under **Security → Roles**.

Each role has:

- **Name and description**
- **Permissions** — the list of operations the role grants (per module / per cluster, where applicable)
- **Built-in flag** — built-in roles cannot be deleted (Administrator, etc.) but their permission set can be customised by editing in place

The grid offers bulk select / bulk grant operations to apply a set of permissions to multiple roles at once.

## Permissions

Permissions are organised by module (AutoSnap, Diagnostic, Resources, …) and by scope:

| Scope | Meaning |
|-------|---------|
| **Application** | Global feature — e.g. *Edit cluster settings*, *Trigger update*. Independent of any specific cluster. |
| **Cluster** | Feature limited to a specific cluster — assign permission *per cluster*. |
| **All clusters** | Feature that applies to every cluster — usually shown only when at least one cluster is configured. |

Editing a role opens a **Permissions Editor** with a per-module accordion: tick the checkboxes for the operations the role can perform. The editor also includes a **Permissions Summary** dialog that lists, in flat form, every operation the role currently grants.

## App Tokens

App Tokens are machine-to-machine credentials (similar to PVE API tokens but for the cv4pve-admin REST API). Create them under **Security → App Tokens**:

| Field | Description |
|-------|-------------|
| **Name** | Identifier for the token (shown in audit logs). |
| **Owner** | Optional — link the token to a specific user. |
| **Expiration** | Optional — token rejected after this date. |
| **Roles / Permissions** | Same model as users: assign roles, or grant individual permissions. |

When you create a token the secret is shown **once** in a one-time dialog — copy it immediately, it will never be shown again.

Use the **App Token Permissions Grid** to fine-tune which paths / clusters a token can access.

## Active Sessions

The **Security → Active Sessions** page lists every currently authenticated user / browser session: user, IP address, user agent, login time, last activity.

Revoke a session with the **Sign out** button on the row — the next request from that browser will be redirected to the login page.

## Audit Logs

Every administrative action (login, role change, cluster create, snapshot job edit, …) is recorded in the **Security → Audit Logs** grid.

Columns include: timestamp, user, action, target cluster, success flag, and a free-text *details* field that captures the JSON payload of the operation. Long details fields are loaded lazily — click the row to expand a card with a copy button and the full pretty-printed body.

### Filtering and retention

- The toolbar offers filters by date range, user, action, success, cluster.
- Retention is controlled in [Maintenance → Cleanup Audit Logs](maintenance.md#cleanup-operations) (default: 180 days).
- Audit logs are stored in the `system` schema of the cv4pve-admin database, separately from Proxmox VE.
