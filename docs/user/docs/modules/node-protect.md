# NodeProtect

Node configuration backup and restore for Proxmox VE clusters.

## Overview

Backs up configuration settings of Proxmox VE nodes for restoration in case of failures.

!!! warning "Requirements"
    Web shell PAM user is required for node configuration backup functionality.

<div class="grid cards" markdown>

-   :material-backup-restore:{ .lg .middle } **Configuration Backup**

    ---

    Backs up node configurations to local folder or Git repository.

-   :material-shield-check:{ .lg .middle } **Configuration Restore**

    ---

    Restores node configurations from backup files.

-   :material-clock-fast:{ .lg .middle } **Backup Management**

    ---

    Manage node configuration backups with manual or scheduled execution using cron expression.

    - <span class="ee"></span> Git provider integration with automatic push

-   :material-connection:{ .lg .middle } **Proxmox VE Integration**

    ---

    Uses web shell to collect node configuration files.

-   :material-check-circle:{ .lg .middle } **Version Control**

    ---

    Track configuration changes over time with Git integration.

-   :material-trending-up:{ .lg .middle } **Recovery Support**

    ---

    Restore node configurations after hardware failure or misconfiguration.

</div>
