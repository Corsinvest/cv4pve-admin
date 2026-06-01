# <span class="ee"></span> :material-webhook: AutoSnap — Web API Hook <span class="scope" data-scope="per-cluster"></span>

Trigger HTTP requests at specific phases of the [AutoSnap](autosnap.md) snapshot lifecycle — integrate with monitoring platforms, orchestration tools, CI/CD pipelines or any HTTP-capable service.

## Features

<div class="grid cards" markdown>

- :material-pulse:{ .lg .middle } **Phase-bound triggers**

    ---

    Bind a hook to a specific point of the snapshot lifecycle (`SnapCreatePre`, `SnapCreatePost`, `SnapRemovePre`, …) — fires exactly when you need it.

- :material-arrange-send-backward:{ .lg .middle } **Multiple hooks per phase**

    ---

    Configure several hooks on the same phase; they run sequentially in a user-defined **Order**.

- :material-code-json:{ .lg .middle } **JSON / XML / Text / GET bodies**

    ---

    POST/PUT/PATCH with `JSON`, `XML` or `Text` body, or simple GET with query parameters — the right shape for the receiver.

- :material-key-variant:{ .lg .middle } **Built-in authentication**

    ---

    None, Basic, Bearer or API Key — set credentials once per hook, no custom header juggling.

- :material-tag-multiple:{ .lg .middle } **Variable placeholders**

    ---

    Reference the current VM ID, name, type, snapshot name, duration or success state via `%CV4PVE_AUTOSNAP_*%` tokens in URL, body or headers.

- :material-alert-octagon:{ .lg .middle } **Fail-fast on errors**

    ---

    A non-2xx response or network error stops execution for that phase and surfaces in the job log — no silent failures.

</div>

## Why

Why webhooks when AutoSnap already runs on schedule?

<div class="why-grid" markdown>

<div markdown>
!!! tip "Quiesce before snapshot"
    Call your app's `/freeze` endpoint on `SnapCreatePre` so the snapshot captures a consistent state — and `/thaw` on `SnapCreatePost` to resume.
</div>

<div markdown>
!!! success "Tell monitoring you're alive"
    Ping a status page or PagerDuty on `SnapJobEnd` — backups silently failing is the #1 cause of "we thought we were protected".
</div>

<div markdown>
!!! info "Wire snapshots into CI/CD"
    Trigger a deploy on `SnapCreatePost` — every release gets a tagged rollback point baked into the pipeline.
</div>

<div markdown>
!!! warning "Audit trail outside PVE"
    Stream every snapshot event to your SIEM / log pipeline — independent record of who-snapshotted-what-when.
</div>

</div>

## Reference

Hooks fire at well-defined phases of the snapshot lifecycle and receive context via `%CV4PVE_AUTOSNAP_*%` placeholders that you can use in URL, body and header values.

??? note reference "All phases"

    | Phase | When |
    |-------|------|
    | `SnapJobStart` | before the entire snapshot job begins |
    | `SnapJobEnd` | after the entire snapshot job completes |
    | `SnapCreatePre` | before creating a snapshot on a specific VM |
    | `SnapCreatePost` | after a snapshot has been successfully created |
    | `SnapCreateAbort` | when snapshot creation fails or is aborted |
    | `CleanJobStart` | before the cleanup (retention) job begins |
    | `CleanJobEnd` | after the cleanup job completes |
    | `SnapRemovePre` | before removing an expired snapshot |
    | `SnapRemovePost` | after an expired snapshot has been removed |
    | `SnapRemoveAbort` | when snapshot removal fails or is aborted |

??? note reference "All placeholders"

    | Placeholder | Value |
    |-------------|-------|
    | `%CV4PVE_AUTOSNAP_PHASE%` | current phase name (e.g. `snap-create-pre`) |
    | `%CV4PVE_AUTOSNAP_VMID%` | VM or container ID |
    | `%CV4PVE_AUTOSNAP_VMNAME%` | VM or container name |
    | `%CV4PVE_AUTOSNAP_VMTYPE%` | `qemu` or `lxc` |
    | `%CV4PVE_AUTOSNAP_LABEL%` | snapshot label defined in the job |
    | `%CV4PVE_AUTOSNAP_KEEP%` | number of snapshots to keep |
    | `%CV4PVE_AUTOSNAP_SNAP_NAME%` | full snapshot name |
    | `%CV4PVE_AUTOSNAP_VMSTATE%` | `1` if VM state is saved, `0` otherwise |
    | `%CV4PVE_AUTOSNAP_DURATION%` | duration of the operation in seconds |
    | `%CV4PVE_AUTOSNAP_STATE%` | `1` if operation succeeded, `0` if failed |

??? note reference "All hook fields"

    Configured per-job in the **Hooks** tab of the job editor.

    | Field | Description |
    |-------|-------------|
    | **Description** | label to identify the hook in logs and UI |
    | **Enabled** | toggle without deleting |
    | **Order** | execution order when multiple hooks share the same phase |
    | **Phase** | the snapshot lifecycle phase that triggers this hook |
    | **URL** | target HTTP endpoint (supports placeholders) |
    | **Method** | `GET`, `POST`, `PUT`, `PATCH`, `DELETE` |
    | **Body Type** | `None`, `JSON`, `XML`, `Text` |
    | **Body** | request body (supports placeholders) |
    | **Headers** | custom HTTP headers (support placeholders) |
    | **Auth** | `None`, `Basic`, `Bearer`, `API Key` |
    | **Timeout (s)** | request timeout (default: 30) |
    | **Ignore SSL** | skip SSL certificate validation |

## Examples

??? note examples "Notify a monitoring system after snapshot creation"

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

??? note examples "Call a webhook with Bearer token authentication"

    - **Phase:** `SnapJobEnd`
    - **Method:** `POST`
    - **URL:** `https://api.example.com/hooks/autosnap`
    - **Auth:** `Bearer` → set your token
    - **Body:** *(empty or custom payload)*

??? note examples "Trigger a GET request with query parameters"

    - **Phase:** `SnapCreatePre`
    - **Method:** `GET`
    - **URL:** `https://example.com/notify?vm=%CV4PVE_AUTOSNAP_VMID%&label=%CV4PVE_AUTOSNAP_LABEL%`

!!! info "How execution works"
    - Hooks within the same phase run **sequentially** in the configured order.
    - A non-2xx HTTP response or network error **stops execution** and writes an error to the job log.
    - The hook result (HTTP status code) is written to the job execution log.
    - Hooks with **Enabled = false** are skipped silently.
