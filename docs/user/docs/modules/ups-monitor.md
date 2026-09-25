# <span class="ee"></span> :material-flash: UPS Monitor <span class="scope" data-scope="per-cluster"></span>

Continuously tracks UPS battery levels, power status, load conditions and runtime estimates — with alerts when the UPS runs on battery or its charge or runtime drops below the configured thresholds.

## Features

<div class="grid cards" markdown>

- :material-pulse:{ .lg .middle } **Real-time Status Monitoring**

    ---

    Continuous tracking of battery charge and voltage, input/output voltage, load percentage, temperature and power source.

- :material-alert-octagon:{ .lg .middle } **Critical Power Alerts**

    ---

    Notifications when the UPS switches to battery, the battery charge falls below the low or shutdown threshold, or the estimated runtime drops below the configured minutes.

- :material-timer-sand:{ .lg .middle } **Runtime Estimation**

    ---

    Reads the UPS's own runtime estimate and alerts when it falls below the configured threshold.

- :material-lan-connect:{ .lg .middle } **SNMP-based Polling**

    ---

    Connects to UPS devices via SNMP v2c with configurable scan schedule. Auto-detection of brand profiles.

- :material-bell-ring:{ .lg .middle } **Notifier Integration**

    ---

    Send alerts via any configured notification channel (Telegram, Slack, email, and more) using the [Notifier](../configuration/admin-area/notifier.md) configuration.

</div>

## Why

Why integrate UPS into cv4pve-admin instead of NUT alone?

<div class="why-grid" markdown>

<div markdown>
!!! tip "Every reading kept"
    Each scan stores status, battery, voltages, load, temperature and runtime — expand a device to see its reading history, retained for **Max Days Logs**.
</div>

<div markdown>
!!! success "Multi-vendor via SNMP"
    Auto-detected device profiles cover the major UPS brands — one module for the whole fleet, no per-vendor agent on every PVE node.
</div>

<div markdown>
!!! info "Alerts before the silence"
    On battery, battery low, runtime dropping — surfaced via Notifier on any configured channel before the power actually goes out.
</div>

<div markdown>
!!! warning "Trends spot a dying battery"
    Battery health degrades slowly. Charts of battery charge, load, input voltage and temperature over time make the "this UPS won't last another outage" call obvious.
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
| **Enabled** | Include the device in the scheduled scan (default: on) |
| **Name** | Friendly name for the UPS device |
| **Host** | IP address of the UPS SNMP agent |
| **Port** | SNMP port (default: 161) |
| **Community** | SNMP v2c community string (default: `public`) |
| **Profile** | SNMP device profile (`auto` = detect the brand from the device, or pick one explicitly) |
| **Location** | Physical location label |
| **Description** | Free-text notes |
| **Managed Nodes** | Proxmox nodes powered by this UPS (empty = all nodes) — informational |
| **Identification** | Manufacturer, Model, Firmware Version, Serial Number — filled from the device with **Retrieve Info** |

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
    | **Default Shutdown Battery Threshold (%)** | 10 | Send a critical alert when battery falls below this level |
    | **Default Shutdown Time Threshold (minutes)** | 5 | Send a critical alert when estimated runtime falls below this value |
    | **Notifier Configurations** | – | List of Notifier configurations to deliver power alerts to |
