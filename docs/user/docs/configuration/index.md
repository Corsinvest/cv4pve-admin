# :material-cog: Configuration

Everything you set up once, before (or while) using the modules.

## Administration

System-wide setup, requires admin privileges.

<div class="grid cards" markdown>

- :material-shield-crown:{ .lg .middle } **[Admin Area](admin-area/index.md)**

    ---

    Proxmox VE clusters, security & access control <span class="ee"></span>, monitoring, maintenance, general settings and notification channels.

- :material-key-chain:{ .lg .middle } **[PVE Permissions](pve-permissions.md)**

    ---

    Required Proxmox VE permissions for each module — quick reference for least-privilege setups.

- :material-file-code:{ .lg .middle } **[appsettings.extra.json](appsettings-extra.md)**

    ---

    Optional file for low-level overrides (database connection, identity policies, logging) that don't have a UI control.

</div>

## Personal

Per-user preferences — every signed-in user can change their own.

<div class="grid cards" markdown>

- :material-account:{ .lg .middle } **[User Profile](profile.md)**

    ---

    Password, two-factor authentication <span class="ee"></span>, language, theme.

</div>

## Where settings live

| Where | What |
|-------|------|
| **Database** | All UI-driven settings: cluster credentials, users, roles, notifier channels, module options. Survives container updates. |
| **`appsettings.extra.json`** | Bootstrap configuration: database connection, log level, identity policies. Read at startup. |
| **`/app/data/`** (Docker volume) | Persistent state: SQLite/Postgres data, exports, attachments, logs. |
| **Environment variables** | Container deployment knobs: `ASPNETCORE_URLS`, timezone, etc. — see the Docker docs. |
