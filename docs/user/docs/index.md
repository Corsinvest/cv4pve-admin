---
hide:
  - navigation
  - toc
---

<div style="text-align: center; margin: 3rem 0;">
  <img src="icon.png" alt="cv4pve-admin" style="width: 120px; height: 120px; margin-bottom: 1rem;">
  <h1 style="font-size: 3rem; font-weight: 700; margin: 0;">cv4pve-admin</h1>
  <p style="font-size: 1.5rem; color: #666; margin-top: 0.5rem;">Professional Administration for Proxmox VE</p>
</div>

<div style="text-align: center; margin: 2rem 0 3rem 0;">
  <a href="installation/" style="display: inline-block; background: #2196F3; color: white; padding: 0.8rem 2rem; border-radius: 4px; text-decoration: none; font-weight: 600; margin: 0.5rem;">Get Started</a>
  <a href="https://github.com/Corsinvest/cv4pve-admin" target="_blank" style="display: inline-block; background: #424242; color: white; padding: 0.8rem 2rem; border-radius: 4px; text-decoration: none; font-weight: 600; margin: 0.5rem;">View on GitHub</a>
  <a href="https://hub.docker.com/r/corsinvest/cv4pve-admin" target="_blank" style="display: inline-block; background: #1488C6; color: white; padding: 0.8rem 2rem; border-radius: 4px; text-decoration: none; font-weight: 600; margin: 0.5rem;">Docker Hub</a>
</div>

---

## By IT Managers, for IT Managers

Proxmox VE manages the hypervisor. **cv4pve-admin manages your infrastructure.**

Built from real-world experience to solve real problems: multi-cluster visibility, proactive monitoring, compliance reporting, and enterprise automation.

**cv4pve-admin doesn't replace Proxmox VE** — it extends it to new frontiers. While Proxmox VE excels at managing individual nodes and VMs, cv4pve-admin addresses the operational challenges IT managers face daily: automated snapshot management across multiple clusters, backup status verification and analytics, replication monitoring, infrastructure diagnostics, compliance reporting, and much more that enterprises need at scale.

**Because infrastructure management shouldn't require manual work cluster by cluster, node by node.**

---

## Why cv4pve-admin?

<div class="grid cards" markdown>

-   :material-monitor-dashboard:{ .lg .middle } **Unified Dashboard**

    ---

    Monitor your entire Proxmox infrastructure from a single, intuitive interface. Real-time metrics, resource analytics, and performance tracking.

-   :material-robot-outline:{ .lg .middle } **AI-Powered Intelligence**

    ---

    Enterprise Edition includes AI assistant for infrastructure insights, automated diagnostics, and intelligent recommendations.

-   :material-clock-check-outline:{ .lg .middle } **Smart Automation**

    ---

    Automated snapshots, backup management, and custom workflows. Set it and forget it with flexible retention policies.

-   :material-chart-line:{ .lg .middle } **Advanced Analytics**

    ---

    Deep insights into backups, replication, storage, and VM performance. Make data-driven decisions.

-   :material-shield-check:{ .lg .middle } **Enterprise Ready**

    ---

    Role-based access control, audit logging, multi-cluster support, and professional technical support.

-   :material-update:{ .lg .middle } **Modern Stack**

    ---

    Built with .NET 9, Blazor, PostgreSQL. Cloud-native architecture with Docker deployment.

</div>

---

## Quick Start

Get up and running in under 2 minutes:

=== "Linux / macOS"

    ```bash
    # One-line installation
    curl -fsSL https://raw.githubusercontent.com/Corsinvest/cv4pve-admin/master/install.sh | bash
    ```

    The installer will:

    - Download Docker Compose configuration
    - Let you choose Community or Enterprise edition
    - Configure PostgreSQL password
    - Start all services automatically

=== "Windows PowerShell"

    ```powershell
    # One-line installation
    irm https://raw.githubusercontent.com/Corsinvest/cv4pve-admin/master/install.ps1 | iex
    ```

    The installer will:

    - Download Docker Compose configuration
    - Let you choose Community or Enterprise edition
    - Configure PostgreSQL password
    - Start all services automatically

=== "Manual Setup"

    ```bash
    # Clone or download the repository
    git clone https://github.com/Corsinvest/cv4pve-admin.git
    cd cv4pve-admin/src/docker

    # Copy and configure environment
    cp .env.example .env
    # Edit .env with your settings

    # Start services
    docker compose -f docker-compose-ce.yaml up -d
    ```

After installation, open your browser to `http://localhost:8080`

[**Detailed Installation Guide →**](installation.md){ .md-button .md-button--primary }

---

## Editions

cv4pve-admin is available in two editions:

- **Community Edition** - Free and open source (AGPL-3.0) with core features
- **Enterprise Edition** - Commercial license with AI, VM Performance, Node Protection, and priority support

[**Compare Editions →**](editions.md){ .md-button .md-button--primary }

---

## Screenshots

<div class="grid" style="grid-template-columns: 1fr 1fr; gap: 1rem; margin: 2rem 0;" markdown>

![Login Screen](images/login.png){ loading=lazy }

![Dashboard Overview](images/screenshot-dashboard.png){ loading=lazy }

![Snapshot Management](images/screenshot-autosnap.png){ loading=lazy }

![Backup Analytics](images/screenshot-backup-analytics.png){ loading=lazy }

</div>

---

## Getting Help

<div class="grid cards" markdown>

-   :material-book-open-variant:{ .lg .middle } **Documentation**

    ---

    Complete guides and API reference

    [:octicons-arrow-right-24: User Guide](user_guide.md)

-   :material-message-question:{ .lg .middle } **Community Support**

    ---

    GitHub Issues and Discussions

    [:octicons-arrow-right-24: Get Help](https://github.com/Corsinvest/cv4pve-admin/issues)

-   :material-email:{ .lg .middle } **Enterprise Support**

    ---

    Priority email

    [:octicons-arrow-right-24: Contact Us](mailto:support@corsinvest.it)

-   :material-web:{ .lg .middle } **Website**

    ---

    More information and resources

    [:octicons-arrow-right-24: Visit corsinvest.it](https://corsinvest.it/cv4pve-admin-proxmox/)

</div>

---

## Open Source & Transparent

cv4pve-admin Community Edition is **100% open source** under AGPL-3.0 license.

- **GitHub Repository:** [Corsinvest/cv4pve-admin](https://github.com/Corsinvest/cv4pve-admin)
- **Docker Images CE:** [corsinvest/cv4pve-admin](https://hub.docker.com/r/corsinvest/cv4pve-admin)
- **Docker Images EE:** [corsinvest/cv4pve-admin-ee](https://hub.docker.com/r/corsinvest/cv4pve-admin-ee)
- **Issue Tracking:** Public issues and feature requests
- **Contributing:** Pull requests welcome

---

<div style="text-align: center; margin: 3rem 0; padding: 2rem; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); border-radius: 8px; color: white;">
  <h2 style="margin-top: 0; color: white;">Ready to Get Started?</h2>
  <p style="font-size: 1.2rem; margin-bottom: 2rem;">Install cv4pve-admin in under 2 minutes</p>
  <a href="installation/" style="display: inline-block; background: white; color: #667eea; padding: 1rem 3rem; border-radius: 4px; text-decoration: none; font-weight: 700; font-size: 1.1rem;">Install Now</a>
</div>

---

<div style="text-align: center; color: #666; font-size: 0.9rem;">
  Developed with ❤️ by <a href="https://www.corsinvest.it">Corsinvest Srl</a><br>
  Professional solutions for Proxmox Virtual Environment<br>
  <br>
  <em>Proxmox® is a registered trademark of Proxmox Server Solutions GmbH.</em>
</div>
