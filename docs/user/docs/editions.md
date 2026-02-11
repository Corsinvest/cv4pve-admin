---
hide:
  #- navigation
  #- toc
---

# Community vs Enterprise Edition

Choose the edition that fits your needs: free and open source Community Edition for personal use, or feature-rich Enterprise Edition for production environments.

---

<div class="grid" style="grid-template-columns: 1fr 1fr; gap: 2rem; margin: 2rem 0;">

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

**Status Legend:** ✅ Available | 🚀 Release Candidate | 🚧 Coming Soon | ❌ Not Available

### Core Modules

| Module | CE | EE | Category | Description | EE Enhancements |
|--------|:--:|:--:|----------|-------------|-----------------|
| **Admin Area** | ✅ | ✅ | System | Cluster management and administration (CE: Single user `admin@local`, no user management) | <ul><li>User management with roles and permissions</li><li>Appearance settings</li><li>System Logs</li><li>Enhanced Audit Logs</li><li>Subscription management</li></ul> |
| **AI Assistant** | ❌ | 🚧 | Utilities | Chat-based AI for cluster automation and diagnostics | — |
| **AI Server (MCP)** | 🚀 | 🚀 | Utilities | Model Context Protocol server | <ul><li>QueryTools</li><li>Historical metrics</li><li>Replications</li><li>Backup jobs</li><li>and more...</li></ul> |
| **AutoSnap** | ✅ | ✅ | Protection | Automated snapshot scheduling with retention policies | — |
| **Backup Analytics** | ✅ | ✅ | Health | Backup job analysis and monitoring | Schedulable analysis with cron |
| **Bots** | ✅ | ✅ | Control | Remote cluster management via Telegram | — |
| **Dashboard** | ✅ | ✅ | Application | Customizable dashboards with 15+ widgets | — |
| **Diagnostic** | ✅ | ✅ | Health | Infrastructure diagnostics and health checks | Schedulable scans with cron |
| **Metrics Exporter** | ✅ | ✅ | Health | Prometheus metrics exporter for monitoring integration | — |
| **Node Protect** | ✅ | ✅ | Protection | Node configuration backup | <ul><li>Schedulable backups with cron</li><li>Git provider integration with automatic push</li></ul> |
| **Notifier** | ✅ | ✅ | Notification | Notification system (CE: Email only) | 119 services (Telegram, Discord, Slack, Teams, and more...) |
| **Profile** | ✅ | ✅ | Application | User profile management | <ul><li>Two-factor authentication (2FA)</li><li>Audit Logs</li></ul> |
| **Replication Analytics** | ✅ | ✅ | Health | Replication job monitoring and analysis | Schedulable analysis with cron |
| **Resources** | ✅ | ✅ | Health | Real-time cluster and resource monitoring | — |
| **System Report** | ✅ | ✅ | Utilities | Comprehensive cluster/VM/node/storage reports | Schedulable reports with cron |
| **Update Manager** | ✅ | ✅ | Health | System update management | <ul><li>Schedulable updates with cron</li><li>Enhanced reporting</li></ul> |
| **UPS Monitor** | ❌ | 🚧 | Protection | Network UPS monitoring via SNMP | — |
| **VM Performance** | ❌ | ✅ | Health | Real-time VM performance tracking with IOPS, latency, bandwidth metrics | — |

### Addon Modules (Enterprise Only)

These are optional add-on modules available exclusively for Enterprise Edition, sold separately or included in specific subscription tiers.

| Addon Module | Status | Category | Description |
|--------------|:------:|----------|-------------|
| **Portal** | ✅ | Management | Multi-tenant MSP portal with role-based access control (RBAC) for service providers |
| **Workflow** | 🚧 | Automation | Visual workflow designer with 30+ Proxmox-specific activities for advanced automation |
| **DDR** | 🚧 | Disaster Recovery | Disaster Recovery orchestration with Ceph RBD Mirror support (Alpha) |
| **DRS** | 🚧 | Resource Mgmt | Distributed Resource Scheduler for intelligent VM load balancing (Alpha) |

---

## Support Options

<div class="grid" style="grid-template-columns: 1fr 1fr; gap: 2rem; margin: 2rem 0;">

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

<div class="grid" style="grid-template-columns: 1fr 1fr; gap: 2rem; margin: 2rem 0;">

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
<a href="https://github.com/Corsinvest/cv4pve-admin/blob/master/LICENSE" class="md-button md-button--primary" style="display: inline-block;"><strong>View License →</strong></a>
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

<div class="grid" style="grid-template-columns: 1fr 1fr; gap: 2rem; margin: 2rem 0;">

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
