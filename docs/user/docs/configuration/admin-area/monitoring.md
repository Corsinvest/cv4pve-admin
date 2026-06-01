# :material-monitor-eye: Monitoring

Observability for the cv4pve-admin host itself (not the Proxmox VE cluster).

## Sections

- **System Information** — process / host metrics, service health
- <span class="ee"></span> **System Logs** — searchable application log viewer
- **Background Jobs** — Hangfire dashboard for scheduled and queued tasks

## System Information

The **Monitoring → System Information** page is a compact dashboard for the cv4pve-admin process:

- CPU and memory usage of the cv4pve-admin process
- Disk usage of the `data/` directory (database, exports, logs)
- Database connection status and round-trip time
- Process uptime and .NET runtime version
- Container indicator (`Docker` vs `Native`)
- Status of background services (Hangfire, notifier workers, …)

Useful when filing a bug report: the **Help → Report a Bug** dialog pre-fills these values into the issue body.

## <span class="ee"></span> System Logs

Application logs are served live under **Monitoring → System Logs**:

- Free-text search and level filter (`Trace` / `Debug` / `Information` / `Warning` / `Error` / `Critical`)
- Time-range filter and download as `.log`
- Rotation is automatic; retention is controlled by [Maintenance → Cleanup System Logs](maintenance.md#cleanup-operations) (default: 30 days)

## Background Jobs

cv4pve-admin uses [Hangfire](https://www.hangfire.io/) to schedule and execute background tasks (diagnostic scans, snapshot jobs, system reports, update scans, notifications, …).

Open the **Hangfire dashboard** from **Monitoring → Background Jobs** (it opens in a new tab). The dashboard groups jobs by state:

| Tab | Meaning |
|-----|---------|
| **Enqueued** | Waiting for a worker. |
| **Scheduled** | Will run at a future point in time (cron / `Schedule(...)`). |
| **Processing** | Currently running. |
| **Succeeded** | Completed successfully. |
| **Failed** | Threw an exception. Click to inspect the stack trace, *Requeue* or *Delete*. |
| **Recurring** | Cron-based jobs (snapshot schedules, scheduled diagnostic scans, etc.). |

From [Maintenance → Cleanup Failed Jobs](maintenance.md#cleanup-operations) you can purge all failed jobs in one click.
