# <img src="icon.png" alt="" height="36" align="top"> cv4pve-admin

```
     ______                _                      __
    / ____/___  __________(_)___ _   _____  _____/ /_
   / /   / __ \/ ___/ ___/ / __ \ | / / _ \/ ___/ __/
  / /___/ /_/ / /  (__  ) / / / / |/ /  __(__  ) /_
  \____/\____/_/  /____/_/_/ /_/|___/\___/____/\__/

Admin for Proxmox VE (Made in Italy)
```

[![License](https://img.shields.io/github/license/Corsinvest/cv4pve-admin.svg?style=flat-square)](LICENSE)
[![Release](https://img.shields.io/github/release/Corsinvest/cv4pve-admin.svg?style=flat-square)](https://github.com/Corsinvest/cv4pve-admin/releases/latest)
[![Docker Pulls CE](https://img.shields.io/docker/pulls/corsinvest/cv4pve-admin?style=flat-square&label=docker%20pulls%20CE)](https://hub.docker.com/r/corsinvest/cv4pve-admin)
[![Docker Pulls EE](https://img.shields.io/docker/pulls/corsinvest/cv4pve-admin-ee?style=flat-square&label=docker%20pulls%20EE)](https://hub.docker.com/r/corsinvest/cv4pve-admin-ee)

> **Web administration for Proxmox VE clusters**: one interface for all your clusters, running outside the nodes and using only the API.
>
> **[Documentation](https://corsinvest.github.io/cv4pve-admin/)**

![cv4pve-admin dashboard](docs/src/assets/images/home-computerscreen.png)

---

## Why

The Proxmox VE web interface works on one cluster at a time, and it answers questions about one object. It does not tell you which VMs no backup job covers, which disks are excluded from backup, whether last night's replications ran, or which guests wait for security updates and a reboot. With several clusters you repeat each check by hand on every one of them.

cv4pve-admin is a web application that sits beside your clusters and does that work: scheduled snapshots, backup and replication analysis, health checks, inventory reports, node configuration backup and more, [one module each](https://corsinvest.github.io/cv4pve-admin/modules/). It doesn't replace Proxmox VE: you keep using it.

It **runs outside the nodes and uses only the Proxmox VE REST API**: nothing to install on the cluster, no system modifications.

---

## Quick start with Docker

cv4pve-admin ships as a Docker image. The installer downloads the Docker Compose files and asks which edition you want.

```bash
# Docker installer, Linux / macOS
curl -fsSL https://raw.githubusercontent.com/Corsinvest/cv4pve-admin/main/install.sh | bash

# Docker installer, Windows PowerShell
irm https://raw.githubusercontent.com/Corsinvest/cv4pve-admin/main/install.ps1 | iex

# Start the containers
cd cv4pve-admin-docker && docker compose up -d
```

Open `http://localhost:8080` (or `http://<server-ip>:8080` from another machine) and sign in with the default credentials `admin@local` / `Password123!`. Requirements and every other detail: [Getting Started](https://corsinvest.github.io/cv4pve-admin/getting-started/).

---

## Documentation

Everything else is in the [documentation](https://corsinvest.github.io/cv4pve-admin/):

- [Modules](https://corsinvest.github.io/cv4pve-admin/modules/): what each module does
- [Community vs Enterprise](https://corsinvest.github.io/cv4pve-admin/editions/): what each edition includes
- [Configuration](https://corsinvest.github.io/cv4pve-admin/configuration/): clusters, users, notifications, settings
- [Changelog](CHANGELOG.md)

> **Looking for v1?** The previous version is at the [v1.3.1 tag](https://github.com/Corsinvest/cv4pve-admin/tree/v1.3.1). v2 is a complete rewrite and is not compatible with v1.

---

**By sysadmins, for sysadmins.**

Part of [cv4pve](https://www.corsinvest.it/en/cv4pve/) suite | Made with ❤️ in Italy by [Corsinvest](https://www.corsinvest.it)

Community Edition: AGPL-3.0, see [LICENSE](LICENSE).

Proxmox® is a registered trademark of Proxmox Server Solutions GmbH. cv4pve is developed by Corsinvest and is not a Proxmox product.

Copyright © Corsinvest Srl
