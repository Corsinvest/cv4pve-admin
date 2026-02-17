# AutoSnap — Web API Hook <span class="ee"></span>

The Web API Hook feature allows you to trigger HTTP requests at specific phases of the AutoSnap snapshot lifecycle. This enables integration with external systems such as monitoring platforms, orchestration tools, CI/CD pipelines, or any HTTP-capable service.

## Overview

Each AutoSnap job can have one or more webhook hooks configured. Each hook is bound to a specific **phase** of the snapshot process and is executed in order when that phase is reached.

If a webhook returns a non-successful HTTP status code, the execution is aborted and an error is raised in the job log.

## Configuration

Hooks are configured per-job in the **Hooks** tab of the job editor.

### Hook fields

| Field | Description |
|-------|-------------|
| **Description** | Label to identify the hook in logs and UI |
| **Enabled** | Enable or disable the hook without deleting it |
| **Order** | Execution order when multiple hooks share the same phase |
| **Phase** | The snapshot lifecycle phase that triggers this hook |
| **URL** | Target HTTP endpoint (supports variable placeholders) |
| **Method** | HTTP method: `GET`, `POST`, `PUT`, `PATCH`, `DELETE` |
| **Body Type** | Request body format: `None`, `JSON`, `XML`, `Text` |
| **Body** | Request body content (supports variable placeholders) |
| **Headers** | Custom HTTP headers (supports variable placeholders) |
| **Auth** | Authentication: `None`, `Basic`, `Bearer`, `API Key` |
| **Timeout (s)** | Request timeout in seconds (default: 30) |
| **Ignore SSL** | Skip SSL certificate validation |

## Phases

Hooks are triggered at the following phases of the AutoSnap lifecycle:

| Phase | Description |
|-------|-------------|
| `SnapJobStart` | Before the entire snapshot job begins |
| `SnapJobEnd` | After the entire snapshot job completes |
| `SnapCreatePre` | Before creating a snapshot on a specific VM |
| `SnapCreatePost` | After a snapshot has been successfully created |
| `SnapCreateAbort` | When snapshot creation fails or is aborted |
| `CleanJobStart` | Before the cleanup (retention) job begins |
| `CleanJobEnd` | After the cleanup job completes |
| `SnapRemovePre` | Before removing an expired snapshot |
| `SnapRemovePost` | After an expired snapshot has been removed |
| `SnapRemoveAbort` | When snapshot removal fails or is aborted |

## Variable placeholders

You can use placeholders in the **URL**, **Body**, and **Header values**. Placeholders are replaced at execution time with the current context values.

Syntax: `%VARIABLE_NAME%`

| Placeholder | Description |
|-------------|-------------|
| `%CV4PVE_AUTOSNAP_PHASE%` | Current phase name (e.g. `snap-create-pre`) |
| `%CV4PVE_AUTOSNAP_VMID%` | VM or container ID |
| `%CV4PVE_AUTOSNAP_VMNAME%` | VM or container name |
| `%CV4PVE_AUTOSNAP_VMTYPE%` | Type: `qemu` or `lxc` |
| `%CV4PVE_AUTOSNAP_LABEL%` | Snapshot label defined in the job |
| `%CV4PVE_AUTOSNAP_KEEP%` | Number of snapshots to keep |
| `%CV4PVE_AUTOSNAP_SNAP_NAME%` | Full snapshot name |
| `%CV4PVE_AUTOSNAP_VMSTATE%` | `1` if VM state is saved, `0` otherwise |
| `%CV4PVE_AUTOSNAP_DURATION%` | Duration of the operation in seconds |
| `%CV4PVE_AUTOSNAP_STATE%` | `1` if operation succeeded, `0` if failed |

## Examples

### Notify a monitoring system after snapshot creation

- **Phase:** `SnapCreatePost`
- **Method:** `POST`
- **URL:** `https://monitoring.example.com/api/events`
- **Body Type:** `JSON`
- **Body:**
```json
{
  "event": "snapshot_created",
  "vmid": "%CV4PVE_AUTOSNAP_VMID%",
  "vmname": "%CV4PVE_AUTOSNAP_VMNAME%",
  "snapshot": "%CV4PVE_AUTOSNAP_SNAP_NAME%",
  "duration": "%CV4PVE_AUTOSNAP_DURATION%"
}
```

### Call a webhook with Bearer token authentication

- **Phase:** `SnapJobEnd`
- **Method:** `POST`
- **URL:** `https://api.example.com/hooks/autosnap`
- **Auth:** `Bearer` → set your token
- **Body:** _(empty or custom payload)_

### Trigger a GET request with query parameters

- **Phase:** `SnapCreatePre`
- **Method:** `GET`
- **URL:** `https://example.com/notify?vm=%CV4PVE_AUTOSNAP_VMID%&label=%CV4PVE_AUTOSNAP_LABEL%`

## Execution behavior

- Hooks within the same phase are executed **sequentially** in the configured order.
- If a hook fails (non-2xx HTTP response or network error), **execution stops** and an error is logged.
- The hook result (HTTP status code) is written to the job execution log.
- Hooks with **Enabled = false** are skipped silently.
