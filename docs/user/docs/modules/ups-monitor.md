# <span class="ee"></span> :material-flash: UPS Monitor <span class="scope" data-scope="per-cluster"></span>

Continuously tracks UPS battery levels, power status, load conditions and runtime estimates — with automated shutdown procedures to protect VMs and hosts during extended power outages.

## Features

<div class="grid cards" markdown>

- :material-pulse:{ .lg .middle } **Real-time Status Monitoring**

    ---

    Continuous tracking of voltage, frequency, load percentage and battery levels.

- :material-alert-octagon:{ .lg .middle } **Critical Power Alerts**

    ---

    Immediate notifications for power failures, low battery and UPS malfunctions.

- :material-timer-sand:{ .lg .middle } **Runtime Estimation**

    ---

    Calculates remaining backup power time based on current load conditions.

- :material-shield-search:{ .lg .middle } **Proactive Warnings**

    ---

    Early alerts for UPS maintenance needs and battery degradation indicators.

- :material-power-off:{ .lg .middle } **Automatic Shutdown**

    ---

    Graceful shutdown procedures for VMs and hosts during extended power outages, with failover-aware ordering to preserve critical systems longest.

- :material-lan-connect:{ .lg .middle } **SNMP-based Polling**

    ---

    Connects to UPS devices via SNMP with configurable scan schedule. Auto-detection of brand profiles.

- :material-bell-ring:{ .lg .middle } **Notifier Integration**

    ---

    Send alerts via any configured notification channel (Telegram, Slack, email, and more) using the [Notifier](../configuration/notifier.md) configuration.

</div>

## Why

Why integrate UPS into cv4pve-admin instead of NUT alone?

<div class="why-grid" markdown>

<div markdown>
!!! tip "Graceful shutdown of VMs, not just hosts"
    Coordinated shutdown sequence preserves critical VMs longest — the orchestration knows about hosts AND guests, not only the host.
</div>

<div markdown>
!!! success "Multi-vendor via SNMP"
    Auto-detected device profiles cover the major UPS brands — one module for the whole fleet, no per-vendor agent on every PVE node.
</div>

<div markdown>
!!! info "Alerts before the silence"
    Battery low, runtime dropping, hardware fault — surfaced via Notifier on any configured channel before the power actually goes out.
</div>

<div markdown>
!!! warning "Trends spot a dying battery"
    Battery health degrades slowly. Charts of runtime/load over time make the "this UPS won't last another outage" call obvious.
</div>

</div>

## Sections

- **Dashboard** — real-time status cards for all monitored UPS devices
- **Devices** — manage UPS device configurations — add, edit, test connectivity, trigger an on-demand scan
- **Trends** — historical charts of power metrics over time

## Devices

Each UPS device is configured with:

| Field | Description |
|-------|-------------|
| **Name** | Friendly name for the UPS device |
| **Host** | SNMP hostname or IP address |
| **Port** | SNMP port (default: 161) |
| **Profile** | SNMP device profile (auto-detected brand) |
| **Location** | Physical location label |
| **Managed Nodes** | Proxmox nodes managed by this UPS (empty = all nodes) |

Per-device actions:

- **Scan Now** — immediately poll the UPS for current readings
- **Test Connection** — verify SNMP connectivity to the device

## Settings

??? note settings "Show all settings"

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **Enabled** | off | Master on/off switch for the scheduled scan |
    | **Cron Expression** | `*/5 * * * *` [:material-open-in-new:](https://crontab.guru/#*/5_*_*_*_*){target=_blank title="Open on crontab.guru"} | When the scheduled scan runs |
    | **Max Days Logs** | 30 | How many days of historical readings to retain |
    | **Default Battery Low Threshold (%)** | 20 | Alert threshold for low battery level |
    | **Default Shutdown Battery Threshold (%)** | 10 | Initiate shutdown when battery falls below this level |
    | **Default Shutdown Time Threshold (minutes)** | 5 | Initiate shutdown when estimated runtime falls below this value |
    | **Notifier Configurations** | – | List of Notifier configurations to deliver power alerts to |
