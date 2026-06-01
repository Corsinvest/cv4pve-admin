# :material-wrench: Maintenance

Housekeeping operations for the cv4pve-admin database, cache and background jobs.

## Sections

- **Fix All** — one-click safe maintenance sequence
- **Database operations** — Reindex, Optimize, Compact (per module schema)
- **Cleanup operations** — Audit Logs, System Logs, Task History, Failed Jobs, Memory Cache
- **Connectivity tests** — verify the app can talk to clusters and the internet

## Fix All

The big red button at the top runs the default safe sequence:

1. Each module's own `FixAsync()` (re-initialises schedules, recreates missing rows, etc.)
2. **Cleanup Audit Logs** — using the configured retention
3. **Reindex** — `REINDEX SCHEMA <module>` for every module
4. **Optimize** — `VACUUM ANALYZE` per table within each module's schema
5. **Clear Memory Cache** — drops cache for all modules, permissions, settings and translations
6. **Cleanup Failed Jobs** — purges failed Hangfire jobs

*Compact* is **not** included in Fix All — it's an opt-in operation (see below).

## Database operations

Three explicit buttons, each running only on the **schema of each module** (never the whole database):

| Button | What it does | Locks? | When to run |
|--------|--------------|:------:|-------------|
| **Reindex** | `REINDEX SCHEMA <module>` per module | Short per-index lock | Routine maintenance, e.g. monthly. |
| **Optimize** | `VACUUM (ANALYZE) <module>.<table>` per table | None | Routine maintenance; refreshes planner statistics. |
| **Compact (full)** | `VACUUM (FULL, ANALYZE) <module>.<table>` per table | **`ACCESS EXCLUSIVE` per table** — app blocks during the run | Only after large cleanups (audit logs, old reports). Asks for explicit confirmation. Reports per-module before/after sizes. |

The Log fieldset at the bottom shows live progress, with a Copy button for sharing and a progress bar above the messages while an operation is running. While any maintenance action is in progress, the other action buttons are disabled to avoid concurrent operations.

!!! warning "Compact locks the database"
    *Compact* runs `VACUUM FULL` which holds an **ACCESS EXCLUSIVE** lock on each table. While it runs the app is effectively offline for the modules involved, and the operation needs temporary disk space roughly equal to your largest table. Use it after a large cleanup (e.g. just after dropping many months of audit logs), not as routine maintenance.

## Cleanup operations

| Button | What it deletes | Retention |
|--------|-----------------|-----------|
| **Cleanup Audit Logs** | Audit log rows older than `N` days | Default: 180 days, editable in the toolbar (1–3650) |
| **Cleanup System Logs** | Application log rows older than `N` days | Default: 30 days |
| **Cleanup Task History** | Background task history rows older than `N` days | Default: 90 days |
| **Cleanup Failed Jobs** | Hangfire failed jobs across all queues | No retention — purges everything |
| **Clear Memory Cache** | In-memory caches: PVE state, permissions, settings, localizations | Immediate |

## Connectivity tests

Two diagnostic buttons that don't modify anything — they just confirm the app can talk to its dependencies:

- **Test Cluster Connections** — opens a PVE client to every enabled cluster and reports round-trip time
- **Test Internet Connectivity** — HTTP `HEAD` to Google, Cloudflare and GitHub from the cv4pve-admin process
