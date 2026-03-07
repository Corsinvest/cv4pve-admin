# Community vs Enterprise Edition

Choose the edition that fits your needs: free and open source Community Edition for personal use, or feature-rich Enterprise Edition for production environments.

---

<div class="grid" style="grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 2rem; margin: 2rem 0;">

<div style="text-align: center; padding: 2rem; border: 2px solid #2196F3; border-radius: 8px; display: flex; flex-direction: column; min-height: 300px;">

<div style="flex-grow: 1;">
<h3 style="margin-top: 0; font-size: 1.5rem;">Community Edition</h3>
<p style="font-weight: 600; margin: 1rem 0;">Free & Open Source</p>
<p>Perfect for home labs, small deployments, and learning</p>
</div>

<div style="margin-top: auto;">
<a href="installation.md" class="md-button md-button--primary" style="display: inline-block;"><strong>Get Started →</strong></a>
</div>

</div>

<div style="text-align: center; padding: 2rem; border: 2px solid #FF9800; border-radius: 8px; display: flex; flex-direction: column; min-height: 300px;">

<div style="flex-grow: 1;">
<h3 style="margin-top: 0; font-size: 1.5rem;">Enterprise Edition</h3>
<p style="font-weight: 600; margin: 1rem 0;">Professional & Advanced</p>
<p><em>Everything in Community Edition, plus:</em></p>
<p>Advanced features • Professional support • Enterprise modules</p>
</div>

<div style="margin-top: auto;">
<a href="https://shop.corsinvest.it" class="md-button md-button--primary" style="display: inline-block;"><strong>Get Enterprise →</strong></a>
</div>

</div>

</div>

---

## Module Comparison

### Product Roadmap & Status

This section outlines the current development status of upcoming and evolving modules.

| Status | Meaning | Description |
|--------|---------|-------------|
| ✅ Available | Stable | Fully supported and production-ready |
| 🚀 Release Candidate | Near Release | Feature-complete, final testing phase |
| 🚧 Coming Soon | In Development | Actively under development |
| 📐 In Design | Planning | Under analysis and design phase |
| ❌ Not Available | N/A | Not planned or discontinued |

### Notes

- Features in 🚀 or 🚧 status may change before final release.
- 📐 modules are subject to roadmap prioritization.
- Enterprise customers may request early access.

### Modules and Features

| Module | CE | EE | Category | Description | EE Enhancements |
|--------|:--:|:--:|----------|-------------|-----------------|
| **[Admin Area](configuration/admin-area.md)** | ✅ | ✅ | System | Cluster management and administration (CE: Single user `admin@local`, no user management) | <ul><li>User management with roles and permissions</li><li>Appearance settings</li><li>System Logs</li><li>Enhanced Audit Logs</li><li>Subscription management</li></ul> |
| **AI Assistant** | ❌ | 🚧 | Utilities | Chat-based AI for cluster automation and diagnostics | |
| **[AI Server (MCP)](modules/ai-server.md)** | ✅ | ✅ | Utilities | Model Context Protocol server for AI integration with 20+ tools for VMs, nodes, storage, backups, replications and metrics | <ul><li>`GetQuerySchema` — schema of all query tables</li><li>`ExecuteQuery` — SQL-like queries on cluster data</li></ul> |
| **[AutoSnap](modules/autosnap.md)** | ✅ | ✅ | Protection | Automated snapshot scheduling with retention policies | <ul><li>Web API Hook: trigger HTTP webhooks on snapshot phase events (before/after create, before/after delete)</li></ul> |
| **[Backup Analytics](modules/backup-analytics.md)** | ✅ | ✅ | Health | Backup job analysis and monitoring | |
| **[Bots](modules/bots.md)** | ✅ | ✅ | Control | Remote cluster management via Telegram | |
| **[Command Palette](modules/command-palette.md)** | ✅ | ✅ | Application | Quick access to commands and navigation (Ctrl+K / Cmd+K) | <ul><li>Additional enterprise commands for user management, subscriptions, and workflow</li></ul> |
| **[Dashboard](modules/dashboard.md)** | ✅ | ✅ | Utilities | Customizable dashboards with widgets and metrics | <ul><li>Additional widgets from Enterprise modules</li></ul> |
| **[Diagnostics](modules/diagnostics.md)** | ✅ | ✅ | Health | Infrastructure diagnostics and health checks | <ul><li>Help links to Proxmox documentation for each issue</li><li>PDF export of diagnostic report</li></ul> |
| **[Metrics Exporter](modules/metrics-exporter.md)** | ✅ | ✅ | Health | Prometheus metrics exporter for monitoring integration | |
| **[Node Protect](modules/node-protect.md)** | ✅ | ✅ | Protection | Node configuration backup | <ul><li>Git provider integration with automatic push</li></ul> |
| **[Notifier](configuration/notifier.md)** | ✅ | ✅ | Notification | Notification system (CE: Email only) | <ul><li>119 services (Telegram, Discord, Slack, Teams, and more...)</li></ul> |
| **[Profile](configuration/profile.md)** | ✅ | ✅ | Application | User profile management | <ul><li>Two-factor authentication (2FA)</li><li>Audit Logs</li></ul> |
| **[Replication Analytics](modules/replication-analytics.md)** | ✅ | ✅ | Health | Replication job monitoring and analysis | |
| **[Resources](modules/resources.md)** | ✅ | ✅ | Health | Real-time cluster and resource monitoring | <ul><li>Additional columns: hostname, OS info</li></ul> |
| **[System Report](modules/system-report.md)** | ✅ | ✅ | Utilities | Comprehensive cluster/VM/node/storage reports | |
| **[Update Manager](modules/update-manager.md)** | ✅ | ✅ | Health | System update management | <ul><li>PDF export of update report</li></ul> |
| **[UPS Monitor](modules/ups-monitor.md)** | ❌ | 🚧 | Protection | Network UPS monitoring via SNMP | |
| **[VM Performance](modules/vm-performance.md)** | ❌ | ✅ | Health | Real-time VM performance tracking with IOPS, latency, bandwidth metrics | |

### Addon Modules (Enterprise Only)

These are optional add-on modules available exclusively for Enterprise Edition, sold separately or included in specific subscription tiers.

| Addon Module | Status | Category | Description |
|--------------|:------:|----------|-------------|
| **[Portal](modules/portal.md)** | ✅ | Management | Multi-tenant MSP portal with role-based access control (RBAC) for service providers |
| **[Workflow](modules/workflow.md)** | 🚧 | Automation | Visual workflow designer with 30+ Proxmox-specific activities for advanced automation |
| **DDR** | 📐 | Protection | Disaster Recovery orchestration with Ceph RBD Mirror support (Alpha) |
| **[DRS](modules/drs.md)** | 🚧 | Automation | Distributed Resource Scheduler for intelligent VM load balancing |

---

## Support Options

<div class="grid" style="grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 2rem; margin: 2rem 0;">

<div style="text-align: center; padding: 2rem; border: 2px solid #2196F3; border-radius: 8px; display: flex; flex-direction: column; min-height: 300px;">

<div style="flex-grow: 1;">
<h3 style="margin-top: 0; font-size: 1.5rem;">Community Edition</h3>
<p style="font-weight: 600; margin: 1rem 0;">Free community support via GitHub:</p>
<p>
Public issue tracking<br>
Community discussions<br>
Documentation and guides<br>
Best-effort response time<br>
No SLA guarantee<br>
</p>
</div>

<div style="margin-top: auto;">
<a href="https://github.com/Corsinvest/cv4pve-admin/issues" class="md-button md-button--primary" style="display: inline-block;"><strong>GitHub Issues →</strong></a>
</div>

</div>

<div style="text-align: center; padding: 2rem; border: 2px solid #FF9800; border-radius: 8px; display: flex; flex-direction: column; min-height: 300px;">

<div style="flex-grow: 1;">
<h3 style="margin-top: 0; font-size: 1.5rem;">Enterprise Edition</h3>
<p style="font-weight: 600; margin: 1rem 0;">Professional Support</p>
<p><em>Community support, plus:</em></p>
<p>
Dedicated support team<br>
Priority issue resolution<br>
Expert technical assistance<br>
</p>
</div>

<div style="margin-top: auto;">
<a href="https://shop.corsinvest.it" class="md-button md-button--primary" style="display: inline-block;"><strong>Get Enterprise →</strong></a>
</div>

</div>

</div>

---

## Licensing

<div class="grid" style="grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 2rem; margin: 2rem 0;">

<div style="text-align: center; padding: 2rem; border: 2px solid #2196F3; border-radius: 8px; display: flex; flex-direction: column; min-height: 400px;">

<div style="flex-grow: 1;">
<h3 style="margin-top: 0; font-size: 1.5rem;">Community Edition</h3>
<p style="font-weight: 600; margin: 1rem 0;">Open Source (AGPL-3.0)</p>
<p>
✅ Free forever<br>
✅ Full source code access<br>
✅ Modify as you need<br>
✅ No usage restrictions<br>
✅ Perfect for home labs & learning<br>
ℹ️ Community support<br>
</p>
<p style="font-weight: 600; margin: 1rem 0;">Best for:</p>
<p>
Personal projects<br>
Small deployments<br>
Testing and development<br>
Learning Proxmox management<br>
</p>
</div>

<div style="margin-top: auto;">
<a href="https://github.com/Corsinvest/cv4pve-admin/blob/main/LICENSE" class="md-button md-button--primary" style="display: inline-block;"><strong>View License →</strong></a>
</div>

</div>

<div style="text-align: center; padding: 2rem; border: 2px solid #FF9800; border-radius: 8px; display: flex; flex-direction: column; min-height: 400px;">

<div style="flex-grow: 1;">
<h3 style="margin-top: 0; font-size: 1.5rem;">Enterprise Edition</h3>
<p style="font-weight: 600; margin: 1rem 0;">Subscription-Based</p>
<p><em>All CE features, plus:</em></p>
<p>
✅ Advanced enterprise modules<br>
✅ Professional support included<br>
✅ Custom integrations<br>
✅ Keep modifications private<br>
✅ Commercial warranty<br>
✅ Flexible subscription plans<br>
</p>
<p style="font-weight: 600; margin: 1rem 0;">Best for:</p>
<p>
Production environments<br>
Business deployments<br>
MSP & service providers<br>
Mission-critical operations<br>
</p>
</div>

<div style="margin-top: auto;">
<a href="https://shop.corsinvest.it" class="md-button md-button--primary" style="display: inline-block;"><strong>Get Enterprise →</strong></a>
</div>

</div>

</div>

---

## Pricing

<div class="grid" style="grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 2rem; margin: 2rem 0;">

<div style="text-align: center; padding: 2rem; border: 2px solid #2196F3; border-radius: 8px; display: flex; flex-direction: column; min-height: 450px;">

<div style="flex-grow: 1;">
<h3 style="margin-top: 0; font-size: 1.5rem;">Community Edition</h3>
<div style="font-size: 3rem; font-weight: 700; color: #2196F3; margin: 1rem 0;">OPEN SOURCE</div>
<p style="font-weight: 600; margin: 1rem 0;">Free Forever</p>
<p>No cost<br>
No license fees<br>
No hidden charges<br>
Single user<br>
Unlimited clusters<br>
No time limits</p>
</div>

<div style="margin-top: auto;">
<a href="installation.md" class="md-button md-button--primary" style="display: inline-block;"><strong>Install Now →</strong></a>
</div>

</div>

<div style="text-align: center; padding: 2rem; border: 2px solid #FF9800; border-radius: 8px; display: flex; flex-direction: column; min-height: 450px;">

<div style="flex-grow: 1;">
<h3 style="margin-top: 0; font-size: 1.5rem;">Enterprise Edition</h3>
<div style="font-size: 3rem; font-weight: 700; color: #FF9800; margin: 1rem 0;">CONTACT US</div>
<p style="font-weight: 600; margin: 1rem 0;">Custom Pricing</p>
<p><em>CE is free forever, EE pricing based on:</em></p>
<p>
Number of Proxmox nodes<br>
Optional addon modules</p>
</div>

<div style="margin-top: auto;">
<a href="https://shop.corsinvest.it" class="md-button md-button--primary" style="display: inline-block;"><strong>Get Enterprise →</strong></a>
</div>

</div>

</div>

---

<div style="text-align: center; color: #666; font-size: 0.9rem;">
  Questions? Contact us at <a href="mailto:support@corsinvest.it">support@corsinvest.it</a>
</div>
