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

    Separate logins for tenants with role-based permissions.

- :material-link-variant:{ .lg .middle } **Native Proxmox Integration**

    ---

    Fully compatible with existing infrastructure — no invasive changes required.

</div>

## Why

Why a portal layer when PVE has its own users and roles?

<div class="why-grid" markdown>

<div markdown>
!!! tip "Customers see only their VMs"
    Tenant users log into a portal that exposes only the VMs assigned to their tenant — no chance of accidentally touching another customer.
</div>

<div markdown>
!!! success "Single pane across clusters"
    A tenant can have VMs across multiple PVE clusters — they see one list, not "log into cluster A, log into cluster B".
</div>

<div markdown>
!!! info "RBAC over PVE permissions"
    Portal roles (Admin / Tenant Admin / Tenant User) layer on top of PVE permissions — define the customer experience once, regardless of underlying cluster ACLs.
</div>

<div markdown>
!!! warning "MSP-grade isolation"
    Customers don't see each other, can't navigate to other tenants' resources, can't enumerate VM IDs that aren't theirs.
</div>

</div>

## Sections

- **Tenants** — create and manage tenants with their assigned VMs/CTs and users

## Tenants

Each tenant contains:

- **VMs / CTs** — list of Proxmox VE resources assigned to the tenant
- **Users** — tenant users with role-based access control

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
