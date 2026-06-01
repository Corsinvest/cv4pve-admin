# Modules

<div class="modules-filter">
  <input type="search" placeholder="Filter modules by name or description…" />
  <div class="chips--all">
    <div class="chips">
      <button data-cat="all">All</button>
      <button data-cat="protection">Protection</button>
      <button data-cat="health">Health</button>
      <button data-cat="control">Control</button>
      <button data-cat="automation">Automation</button>
      <button data-cat="utilities">Utilities</button>
      <button data-cat="management">Management</button>
      <button data-cat="features">Features</button>
    </div>
    <div class="chips chips--edition">
      <button data-edition="all">All</button>
      <button data-edition="ce">CE</button>
      <button data-edition="ee">EE</button>
    </div>
  </div>
</div>

<div class="grid cards modules-grid" markdown>

- :material-camera:{ .lg .middle } **[AutoSnap](autosnap.md)**

    ---

    Automated snapshot scheduling and management <span class="scope" data-scope="per-cluster"></span>


- :material-shield-check:{ .lg .middle } **[Node Protect](node-protect.md)**

    ---

    Automated node configuration backup and restore <span class="scope" data-scope="per-cluster"></span>


- :material-flash:{ .lg .middle } **[UPS Monitor](ups-monitor.md)**

    ---

    UPS monitoring with automated shutdown and power alerts <span class="ee"></span> <span class="scope" data-scope="per-cluster"></span>


- :material-cog-sync:{ .lg .middle } **DDR**

    ---

    📐 In design — disaster recovery orchestration with Ceph RBD Mirror <span class="ee"></span>


- :material-backup-restore:{ .lg .middle } **[Backup Analytics](backup-analytics.md)**

    ---

    Backup job analysis and trend insights <span class="scope" data-scope="per-cluster"></span>


- :material-sync:{ .lg .middle } **[Replication Analytics](replication-analytics.md)**

    ---

    Replication job monitoring and analysis <span class="scope" data-scope="per-cluster"></span>


- :material-stethoscope:{ .lg .middle } **[Diagnostics](diagnostics.md)**

    ---

    Automated cluster health checks, diagnostics and compliance checks <span class="scope" data-scope="per-cluster"></span>


- :material-check-decagram:{ .lg .middle } **[Resources](resources.md)**

    ---

    Real-time cluster and resource status monitoring <span class="scope" data-scope="per-cluster"></span>


- :material-chart-line:{ .lg .middle } **[Metrics Exporter](metrics-exporter.md)**

    ---

    Exposes Proxmox VE metrics for Prometheus <span class="scope" data-scope="per-cluster"></span>


- :material-update:{ .lg .middle } **[Update Manager](update-manager.md)**

    ---

    System updates scanning and management <span class="scope" data-scope="per-cluster"></span>


- :material-monitor-dashboard:{ .lg .middle } **[VM Performance](vm-performance.md)**

    ---

    Real-time VM performance tracking — IOPS, latency, bandwidth <span class="ee"></span> <span class="scope" data-scope="per-cluster"></span>


- :material-robot:{ .lg .middle } **[Bots](bots.md)**

    ---

    Remote cluster management via Telegram chatbot <span class="scope" data-scope="per-cluster"></span>


- :material-graph:{ .lg .middle } **[Workflow](workflow.md)**

    ---

    Visual workflow designer for event-driven automation <span class="ee"></span> <span class="scope" data-scope="per-cluster"></span>


- :material-hub:{ .lg .middle } **[AI Server](ai-server.md)**

    ---

    Model Context Protocol server for AI integration <span class="scope" data-scope="all-clusters"></span>


<!-- TODO: AI Assistant is not yet released — re-enable this card when the module ships.
- :material-auto-awesome:{ .lg .middle } **[AI Assistant](ai-assistant.md)**

    ---

    Built-in chat assistant accessible from the main header <span class="ee"></span> <span class="scope" data-scope="all-clusters"></span>
-->


- :material-file-document:{ .lg .middle } **[System Report](system-report.md)**

    ---

    Comprehensive cluster, VM, node and storage reports <span class="scope" data-scope="per-cluster"></span>


- :material-dns:{ .lg .middle } **[Portal](portal.md)**

    ---

    Multi-tenant MSP portal with role-based access control (RBAC) <span class="ee"></span> <span class="scope" data-scope="all-clusters"></span>


- :material-view-dashboard:{ .lg .middle } **[Dashboard](dashboard.md)**

    ---

    Customizable dashboards with widgets and metrics — home page after login <span class="scope" data-scope="all-clusters"></span>


- :material-magnify:{ .lg .middle } **[Command Palette](command-palette.md)**

    ---

    Quick access to commands and navigation (Ctrl+K / Cmd+K) <span class="scope" data-scope="all-clusters"></span> <span class="scope" data-scope="per-cluster"></span>


</div>

---

!!! tip "Module configuration"
    All modules have a configuration section accessible by clicking the :material-cog: gear icon in the top right corner of the module page.

!!! info "Scope: per-cluster vs all-clusters"
    Each module is marked with its scope next to the title:

    - <span class="scope" data-scope="per-cluster"></span> modules are only visible when a **specific cluster** is selected in the top cluster picker. Their configuration is separate per cluster.
    - <span class="scope" data-scope="all-clusters"></span> modules are always available, regardless of which cluster (or **All Clusters**) is selected.
