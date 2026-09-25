# <span class="ee"></span> :material-dns: Portal <span class="scope" data-scope="all-clusters"></span>

Multi-tenant management portal for Proxmox VE environments, designed for MSPs and IT teams. Create, assign and isolate virtual environments for clients, departments or groups while keeping centralised control over the cluster.

## Features

<div class="grid cards" markdown>

- :material-account-group:{ .lg .middle } **Isolated Tenant Management**

    ---

    Each tenant can access only their own virtual resources.

- :material-hub:{ .lg .middle } **Centralized Control**

    ---

    Assign resources, policies and access from a single point.

- :material-trending-up:{ .lg .middle } **Easy Scalability**

    ---

    Quickly add new tenants without impacting the existing infrastructure.

- :material-monitor-dashboard:{ .lg .middle } **Intuitive Interface**

    ---

    Clear and well-organised frontend for administrators and tenants.

- :material-shield-lock:{ .lg .middle } **Secure and Segmented Access**

    ---

    Tenant users get their own cv4pve-admin accounts with per-VM permissions.

- :material-link-variant:{ .lg .middle } **Native Proxmox Integration**

    ---

    Fully compatible with existing infrastructure — no invasive changes required.

</div>

## Why

Why a portal layer when PVE has its own users and roles?

<div class="why-grid" markdown>

<div markdown>
!!! tip "Customers see only their VMs"
    Tenant users log into cv4pve-admin and are granted permissions only on the VMs assigned to their tenant — no chance of accidentally touching another customer.
</div>

<div markdown>
!!! success "One tenant, one cluster"
    Each tenant is bound to a single cluster; its VMs/CTs are picked from that cluster's resources.
</div>

<div markdown>
!!! info "cv4pve-admin permissions, not PVE ACLs"
    Access is granted through cv4pve-admin's own per-VM permissions — no users or ACLs to create on the Proxmox VE cluster.
</div>

<div markdown>
!!! warning "MSP-grade isolation"
    Customers don't see each other, can't navigate to other tenants' resources, can't enumerate VM IDs that aren't theirs.
</div>

</div>

## Sections

- **Tenants** — create and manage tenants with their assigned VMs/CTs and users

## Tenants

Each tenant belongs to one cluster and contains:

- **VMs / CTs** — list of Proxmox VE resources (from the tenant's cluster) assigned to the tenant
- **Users** — tenant users: regular cv4pve-admin accounts (a new user is created with a random password and receives a confirmation email). For each VM/CT of the tenant you pick which permissions the user gets: Read, Audit, Console, Power Management, Replication manager, Replication Schedule Now, Snapshot manager, Snapshot Rollback, Backup manager, Backup Restore, Backup Restore File. A user can also be flagged as **Tenant Admin**

### Roles

| Role | Description |
|------|-------------|
| **Portal Admin** | Full access to tenant management, VMs and users |
| **Portal Tenant Admin** | Manages resources within their own tenant |
| **Portal Tenant User** | Read-only or limited access within their tenant |

## Use Cases

- **MSP / Service Providers** — separate environments per customer with isolated access
- **Enterprise IT** — department-level resource isolation on a shared cluster
- **Education** — assign lab environments to student groups
