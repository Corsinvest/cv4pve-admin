# Portal

<span class="ee"></span> Multi-tenant management portal for Proxmox VE environments, designed for MSPs and IT teams.

## Overview

**Portal** is a multi-tenant management solution that allows you to create, assign, and isolate virtual environments for clients, departments, or specific groups, ensuring control and security.
It provides centralized tenant management while maintaining the full flexibility of the Proxmox VE cluster.

---

## Key Features

- **Isolated Tenant Management** — Each tenant can access only their own virtual resources.
- **Centralized Control** — Assign resources, policies, and access from a single point.
- **Easy Scalability** — Quickly add new tenants without impacting the existing infrastructure.
- **Intuitive Interface** — Clear and well-organized frontend for administrators and tenants.
- **Secure and Segmented Access** — Separate logins for tenants with specific roles and permissions.
- **Native Proxmox Integration** — Fully compatible with existing infrastructure, no invasive changes required.

---

## Pages

| Page | Description |
|------|-------------|
| **Overview** | Summary of tenant status and resources |
| **Tenants** | Create and manage tenants with their VMs and users |

---

## Tenants

Each tenant contains:

- **VMs** — list of Proxmox VE virtual machines and containers assigned to the tenant
- **Users** — tenant users with role-based access control

### Roles

| Role | Description |
|------|-------------|
| **Portal Admin** | Full access to tenant management, VMs, and users |
| **Portal Tenant Admin** | Manages resources within their own tenant |
| **Portal Tenant User** | Read-only or limited access within their tenant |

---

## Use Cases

- **MSP / Service Providers** — Separate environments per customer with isolated access
- **Enterprise IT** — Department-level resource isolation on a shared cluster
- **Education** — Assign lab environments to student groups
