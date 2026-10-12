# Proxmox VE test nodes

A development tool: Proxmox VE nodes that run in containers, to try what cannot be tried on a real cluster.
A node can be shut down, rebooted or broken without harm: a shutdown command stops its container, and
`docker start` brings the node back.

- `pve-single`: one node on its own.
- `pve-1`, `pve-2`, `pve-3`: three nodes, made into the cluster `cvtest` by `create-cluster.ps1`.

Nothing in the application starts them. The containers run with every capability: use them only on a
development machine.

## Image

`ghcr.io/longqt-sea/proxmox-ve`, from <https://github.com/LongQT-sea/containerized-proxmox> (GPL-3.0), a
community project that runs Proxmox VE in a container. It is not part of cv4pve-admin and is not
distributed with it: the compose file only refers to it, pinned by digest. The digest is the one tried on
2026-10-12, Proxmox VE 9.2 (pve-manager 9.2.21).

## Use

```powershell
docker compose up -d       # start the four nodes
./create-cluster.ps1       # once: make pve-1, pve-2 and pve-3 into the cluster "cvtest"
docker start pve-3         # switch a node on again after a shutdown
docker compose down -t 0   # remove everything
```

The containers have no volumes. `docker compose stop` and `start` keep the cluster; after
`docker compose down` it is gone, and `create-cluster.ps1` is run again. The script can be run more than
once: it skips what is already done.

| | Address | Nodes |
|---|---|---|
| Single node | `https://127.0.0.1:18006` | `pve-single` |
| Cluster `cvtest` | `https://127.0.0.1:18011`, `:18012`, `:18013` | `pve-1`, `pve-2`, `pve-3` |

User `root@pam`, password `proxmox123`, self-signed certificates. The password is public and the ports are
published on the loopback address only: do not change that.

In cv4pve-admin add the single node and the cluster as two clusters of their own.

## What was tried

On 2026-10-12, with Docker Desktop on Windows (WSL 2, kernel 6.18):

- the cluster of three nodes is created from the command line and has quorum;
- `POST /nodes/pve-3/status` with `command=shutdown`, sent to `pve-1`: the container of `pve-3` stops
  within a few seconds and the cluster shows the node offline;
- `docker start pve-3`: the node joins the cluster again.

Not tried:

- guests: no virtual machine or container was created in a node. A virtual machine needs `/dev/kvm` to
  start;
- other hosts: Docker Engine on Linux, macOS.

Not available:

- high availability, which needs a hardware watchdog the containers do not have.
