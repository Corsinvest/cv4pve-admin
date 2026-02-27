# User Guide

## Command Palette

Press `Ctrl+K` (or `Cmd+K` on macOS) to open the Command Palette from anywhere in the application.

The Command Palette provides fast access to navigation, search, and commands — without using the mouse.

### Keyboard Shortcuts

| Key | Action |
|-----|--------|
| `Ctrl+K` / `Cmd+K` | Open Command Palette |
| `↑` `↓` | Navigate results |
| `Enter` | Select / execute |
| `Esc` | Close |

### Search

Type any text to search across modules, VMs, nodes, storages, and pools.

When the palette opens with an **empty query**, it shows suggested modules and available commands.

### Filter Prefixes

Use prefixes to narrow results to a specific resource type. Filters require a cluster to be selected.

| Prefix | Description | Example |
|--------|-------------|---------|
| `vm:` | Filter by Virtual Machine or Container | `vm:101`, `vm:debian` |
| `node:` | Filter by Node | `node:pve1` |
| `storage:` | Filter by Storage | `storage:local` |
| `pool:` | Filter by Pool | `pool:dev` |
| `ip:` | Filter by IP address (requires OS info) | `ip:192.168` |

Filters can be combined with free text. For example: `vm: debian` searches for VMs matching "debian".

### Commands (`>`)

Type `>` to switch to **command mode**. Commands execute actions directly from the palette.

| Command | Description |
|---------|-------------|
| `> start` | Start a VM/Container (shows a VM picker) |
| `> stop` | Stop a VM/Container |
| `> restart` | Restart a VM/Container |
| `> console` | Open console for a running VM/Container |
| `> create snapshot` | Create a snapshot (name, description, RAM state) |
| `> logout` | Logout from the application |

!!! note
    Commands that require parameters open a dialog to collect input before executing.

!!! warning "Console command — API Token limitation"
    The `> console` command is not available when the cluster uses **API Token** authentication.
    A notification will inform you to switch to **Credential** access type to enable this feature.
