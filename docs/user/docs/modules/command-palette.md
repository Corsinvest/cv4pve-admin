# Command Palette

The Command Palette provides fast keyboard-driven access to modules, navigation, and Proxmox VE operations — without leaving your current page.

## Opening the Palette

| Platform | Shortcut |
|----------|----------|
| Windows / Linux | `Ctrl+K` |
| macOS | `Cmd+K` |

You can also open it by clicking the search bar in the top header.

---

## Navigation

| Key | Action |
|-----|--------|
| `↑` / `↓` | Move through results |
| `Enter` | Execute selected item |
| `Esc` | Close palette |

---

## Search Filters

Prefix your query to narrow results to a specific category:

| Prefix | Filters |
|--------|---------|
| `vm:` | VM / Container by ID (e.g. `vm:100`) |
| `node:` | Node by name (e.g. `node:pve1`) |
| `ip:` | VM / Container by IP address or hostname (e.g. `ip:192.168`) |
| `>` | Commands only (e.g. `>start`, `>logout`) |

Without a prefix, the search spans all categories: modules, VMs, nodes, and commands.

---

## Commands

Commands are executable actions. Those that require parameters open a dialog before execution.

### System

| Command | Description |
|---------|-------------|
| `logout` | Log out the current user |

### Proxmox VE

| Command | Description | Parameters |
|---------|-------------|------------|
| `start` | Start a stopped VM / Container | VM / CT selector |
| `stop` | Stop a running VM / Container | VM / CT selector |
| `restart` | Restart a running VM / Container | VM / CT selector |
| `console` | Open web console for a running VM / Container | VM / CT selector |
| `create snapshot` | Create a snapshot | VM / CT selector, name, description (optional), include RAM (optional) |

!!! note "Console requirement"
    The `console` command requires a **PAM user** with credential-based access. It is not available with API Token authentication.

---

## Results

Each result shows:

- **Icon** — colored by category (green = VM, blue = module, …)
- **Title** — VM ID, module name, or command prefix
- **Subtitle** — description or additional info
- **Status badges** — running / stopped, hostname, OS version, lock state
- **Action indicator** — dialog icon if parameters are required, or instant execution icon

---

## Result Types

| Type | Description |
|------|-------------|
| **Module** | Application modules and navigation links |
| **VM / CT** | Proxmox VE virtual machines and containers |
| **Node** | Proxmox cluster nodes |
| **Command** | Executable actions |

---

## Permissions & Cluster Context

- Results and commands respect the current user's permissions.
- Modules that require a specific cluster are hidden when **All Clusters** is selected.
- Proxmox VE commands (start, stop, …) operate on the **currently selected cluster**.

---

## Roadmap

More commands and search providers are planned for future releases, including additional Proxmox VE operations, storage management actions, and expanded search filters.
