# Proxmox VE Permissions

cv4pve-admin connects to Proxmox VE via API and requires specific permissions depending on which modules and features you use.

---

## Recommended: PVEAdmin role

The simplest and recommended setup is to assign the built-in **`PVEAdmin`** role at path `/` to the Proxmox user or API token used by cv4pve-admin.

This grants full functionality for all modules without any further configuration.

!!! tip "Automatic API Token Creation"
    cv4pve-admin can create a dedicated Proxmox user and API token for you automatically.
    [:octicons-arrow-right-24: Learn more](admin-area.md#automatic-api-token-creation)

---

## Minimum permissions

If you prefer a least-privilege setup, assign a **custom Proxmox role** at path `/` with only the permissions required for the features you use.

### Base (required for all modules)

These are the permissions included in the built-in `PVEAuditor` role — equivalent and simpler to assign directly.

| Permission | Purpose |
|------------|---------|
| `Sys.Audit` | Cluster status, node info, tasks, firewall, HA |
| `VM.Audit` | VM/CT list, status, config |
| `Datastore.Audit` | Storage list, status, content |
| `Pool.Audit` | Resource pool list |
| `SDN.Audit` | SDN zones, VNets, subnets |

### Additional permissions by module

| Module | Additional permissions _(beyond base)_ |
|--------|----------------------------------------|
| **Dashboard**<br>**Resources** | • `VM.PowerMgmt` _(start/stop/reboot buttons)_<br>• `VM.Console` _(console button)_<br>• `VM.GuestAgent.Audit` _(hostname/IP info)_ |
| **Replication Analytics** | — |
| **Backup Analytics** | — |
| **Metrics Exporter** | — |
| **Diagnostics** | — |
| **System Report** | — |
| **VM OS Info** | • `VM.GuestAgent.Audit` |
| **AutoSnap** | • `VM.Snapshot`<br>• `VM.Snapshot.Rollback`<br>• `Datastore.AllocateSpace`<br>• `Pool.Allocate`<br>• `VM.PowerMgmt` _(only if Include RAM is enabled)_ |
| **Node Protect** | Base only — uses SSH to operate on nodes |
| **Update Manager** | Base only — uses SSH to operate on nodes/VMs |
| **UPS Monitor** | — _(SNMP only, no PVE API)_ |
| **VM Performance** | • `VM.Monitor` |
| **Portal** | • `VM.PowerMgmt`<br>• `VM.Console`<br>• `VM.Snapshot`<br>• `VM.Snapshot.Rollback`<br>• `VM.Backup`<br>• `Datastore.AllocateSpace`<br>_(depends on tenant permissions configured)_ |
| **Workflow** | Depends on configured activities:<br>• Clone: `VM.Clone`, `Datastore.AllocateSpace`<br>• Backup: `VM.Backup`, `Datastore.AllocateSpace`<br>• Resize disk: `VM.Config.Disk`<br>• HA operations: `Sys.Modify`<br>• Node reboot/shutdown: `Sys.PowerMgmt`<br>• Convert to template: `VM.Allocate` |

---

## Example: monitoring + AutoSnap custom role

```
Sys.Audit
VM.Audit
VM.Snapshot
VM.Snapshot.Rollback
Datastore.Audit
Datastore.AllocateSpace
Pool.Audit
Pool.Allocate
```

Assign at path `/` to your cv4pve-admin user or API token.

!!! warning
    Features that lack required permissions will show errors or empty data — other features will continue to work normally.
