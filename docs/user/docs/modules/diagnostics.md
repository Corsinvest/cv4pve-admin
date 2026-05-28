# Diagnostics

System health checks and infrastructure diagnostics for Proxmox VE.

## Overview

Performs health checks and analysis of Proxmox VE environments. Scans clusters, nodes, and virtual machines to identify configuration issues and performance problems.

<div class="grid cards" markdown>

-   :material-calendar-clock:{ .lg .middle } **Health Checks**

    ---

    Run diagnostic scans on Proxmox VE infrastructure with on-demand or scheduled execution using cron expression.

-   :material-alert-circle:{ .lg .middle } **Issue Detection**

    ---

    Identifies misconfigurations, hardware problems, and performance bottlenecks.

-   :material-file-document-multiple:{ .lg .middle } **System Analysis**

    ---

    Examines system logs, configurations, and resource utilization patterns.

-   :material-history:{ .lg .middle } **Issue History**

    ---

    Maintains history of discovered problems with option to ignore known issues.

-   :material-connection:{ .lg .middle } **Proxmox VE Integration**

    ---

    Uses Proxmox VE API to collect system information.

-   :material-lightbulb-on:{ .lg .middle } **Issue Details**

    ---

    Displays issue descriptions with resolution recommendations.

-   :material-download:{ .lg .middle } **Export Reports**

    ---

    Download scan results as **PDF** (issues table colored by gravity, ignored issues section, footer) or **Excel** (autofilter-enabled sheet for slicing/pivoting). Format chosen from a split-button dropdown on the Scans page.

-   :material-link-variant:{ .lg .middle } **Help Links**

    ---

    Each issue can link to the relevant Proxmox documentation page (e.g. Qemu Guest Agent, VirtIO drivers, end-of-life OS tracking).

-   :material-chart-box:{ .lg .middle } **Executive Summary** <span class="ee"></span>

    ---

    Enterprise PDF adds a one-page Executive Summary at the top: issue counts by gravity (Critical / Warning / Info) and the top 5 critical issues — useful for management or MSP customer reports.

</div>
