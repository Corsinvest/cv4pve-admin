# UPS Monitor

<span class="ee"></span> Comprehensive monitoring of Uninterruptible Power Supply systems in Proxmox VE environments.

## Overview

**UPS Monitor** continuously tracks UPS battery levels, power status, load conditions, and runtime estimates during power outages.
Administrators get real-time visibility into power infrastructure health to prevent unexpected system failures, with automated shutdown procedures to protect VMs and hosts during extended power outages.

---

## Key Features

- **Real-time Status Monitoring** — Continuous tracking of voltage, frequency, load percentage, and battery levels
- **Critical Power Alerts** — Immediate notifications for power failures, low battery, and UPS malfunctions
- **Runtime Estimation** — Calculates remaining backup power time based on current load conditions
- **Proactive Warnings** — Early alerts for UPS maintenance needs and battery degradation indicators
- **Automatic Shutdown** — Graceful shutdown procedures for VMs and hosts during extended power outages
- **Failover Coordination** — Coordinated shutdown sequences to preserve critical systems longest
- **SNMP-based Polling** — Connects to UPS devices via SNMP with configurable scan schedule
- **Notifier Integration** — Send alerts via any configured notification channel (Telegram, Slack, email, and more)

---

## Pages

| Page | Description |
|------|-------------|
| **Overview** | Summary of UPS Monitor capabilities |
| **Dashboard** | Real-time status cards for all monitored UPS devices |
| **Devices** | Manage UPS device configurations |
| **Trends** | Historical charts of power metrics over time |

---

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

Actions available per device:

- **Scan Now** — Immediately poll the UPS for current readings
- **Test Connection** — Verify SNMP connectivity to the device

---

## Configuration

| Setting | Description |
|---------|-------------|
| **Enabled** | Enable or disable automatic scanning |
| **Schedule (Cron)** | Configurable cron expression for automatic scans |
| **Max Days Logs** | Number of days to retain historical readings |
| **Default Battery Low Threshold (%)** | Alert threshold for low battery level |
| **Default Shutdown Battery Threshold (%)** | Initiate shutdown when battery falls below this level |
| **Default Shutdown Time Threshold (minutes)** | Initiate shutdown when estimated runtime falls below this value |
| **Notifier** | Notification channel for power alerts |
