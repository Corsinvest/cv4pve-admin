# Update Manager

System updates scanning and management.

## Overview

Scans VM/CT in Proxmox VE to identify available updates including security patches. Provides visibility into pending updates without making changes.

!!! warning "Requirements"
    Web shell PAM user is required for update scanning functionality.

<div class="grid cards" markdown>

-   :material-security:{ .lg .middle } **Security Patches**

    ---

    Identify critical security patches across VMs and containers.

-   :material-package-variant:{ .lg .middle } **Update Inventory**

    ---

    View available updates by system, package, and severity level.

-   :material-file-check:{ .lg .middle } **Compliance Reports**

    ---

    Generate reports showing patch status for security audits.

-   :material-calendar-clock:{ .lg .middle } **Update Scanning**

    ---

    Scan systems for available updates on-demand or scheduled using cron expression.

    - <span class="ee"></span> Enhanced reporting

-   :material-shield-search:{ .lg .middle } **Read-Only Scanning**

    ---

    Check for updates without making system changes.

-   :material-text-box-check:{ .lg .middle } **Status Reports**

    ---

    View update status with priority indicators for critical patches.

</div>
