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
