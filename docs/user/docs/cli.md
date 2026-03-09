# CLI Reference

cv4pve-admin includes a built-in command-line interface for administrative tasks that can be performed without accessing the web UI.

## Usage

Run the application with `--help` to list all available commands:

```bash
cv4pve-admin --help
```

Each command also supports `--help` for details on its options:

```bash
cv4pve-admin user --help
cv4pve-admin user reset-password --help
```

## User commands

All user commands accept `-u <username>` (default: `admin@local`).

| Command | Description |
|---------|-------------|
| `user reset-password -u <user> -p <password>` | Reset user password |
| `user enable -u <user>` | Enable a disabled user account |
| `user disable -u <user>` | Disable a user account |
| `user unlock -u <user>` | Unlock a locked out user account |

## Troubleshooting

### Reset a forgotten admin password

If you are locked out of the web UI, you can reset the password directly via CLI.

**Docker Compose:**

```bash
docker compose run --rm cv4pve-admin user reset-password -u admin@local -p NewPassword123!
```

**Binary:**

```bash
cv4pve-admin user reset-password -u admin@local -p NewPassword123!
```

!!! warning
    Use a strong password and change it again from the web UI after recovering access.

### Unlock a locked out account

After too many failed login attempts, the account is locked. To unlock it:

**Docker Compose:**

```bash
docker compose run --rm cv4pve-admin user unlock -u admin@local
```

**Binary:**

```bash
cv4pve-admin user unlock -u admin@local
```
