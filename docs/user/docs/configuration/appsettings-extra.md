# Application Settings

This document describes all configurable settings available in `appsettings.extra.json`.

!!! info "Configuration File Location"
    The `appsettings.extra.json` file is automatically loaded by cv4pve-admin if present in the `config/` directory (mounted as `/app/config/` in Docker). This optional configuration file allows you to override default settings without modifying the main `appsettings.json` file. If the file does not exist, the application starts normally with default settings.

## Connection Strings

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=cv4pve_admin;Username=postgres;Password=yourpassword"
  }
}
```

| Setting | Description |
|---------|-------------|
| `DefaultConnection` | PostgreSQL connection string used by Entity Framework, Hangfire, and Serilog |

## Cookie Settings

```json
{
  "CookieSettings": {
    "ExpireDays": 14
  }
}
```

| Setting | Default | Description |
|---------|---------|-------------|
| `ExpireDays` | 14 | Number of days before authentication cookie expires |

## Security Settings

### Password Options

```json
{
  "Security": {
    "PasswordOptions": {
      "RequiredLength": 8,
      "RequireNonAlphanumeric": true,
      "RequireDigit": true,
      "RequireLowercase": true,
      "RequireUppercase": true
    }
  }
}
```

| Setting | Default | Description |
|---------|---------|-------------|
| `RequiredLength` | 6 | Minimum password length |
| `RequireNonAlphanumeric` | true | Require special characters (!@#$%^&* etc.) |
| `RequireDigit` | true | Require at least one digit (0-9) |
| `RequireLowercase` | true | Require at least one lowercase letter |
| `RequireUppercase` | true | Require at least one uppercase letter |

### Lockout Options

```json
{
  "Security": {
    "LockoutOptions": {
      "MaxFailedAccessAttempts": 5,
      "AllowedForNewUsers": true,
      "DefaultLockoutTimeSpan": "00:15:00"
    }
  }
}
```

| Setting | Default | Description |
|---------|---------|-------------|
| `MaxFailedAccessAttempts` | 5 | Number of failed login attempts before lockout |
| `AllowedForNewUsers` | true | Enable lockout for new users |
| `DefaultLockoutTimeSpan` | 00:15:00 | Duration of account lockout (format: HH:mm:ss) |

## Serilog Configuration

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Warning",
        "System": "Warning"
      }
    },
    "Properties": {
      "Application": "cv4pve-admin"
    },
    "WriteTo": [...]
  }
}
```

### Log Levels

- `Verbose` - Most detailed logging
- `Debug` - Debugging information
- `Information` - General information (default)
- `Warning` - Warnings and potential issues
- `Error` - Errors that need attention
- `Fatal` - Critical failures

### File Sink

```json
{
  "Name": "File",
  "Args": {
    "path": "./data/logs/log-.txt",
    "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {ClientIp} {UserName}{NewLine}{Exception}",
    "rollingInterval": "Day",
    "retainedFileCountLimit": 30
  }
}
```

| Setting | Description |
|---------|-------------|
| `path` | Log file path (supports rolling with date placeholder) |
| `outputTemplate` | Log message format |
| `rollingInterval` | Rolling interval (Hour, Day, Month, Year) |
| `retainedFileCountLimit` | Number of log files to retain |

### PostgreSQL Sink (Enterprise Edition)

```json
{
  "Name": "PostgreSQL",
  "Args": {
    "connectionString": "DefaultConnection",
    "tableName": "Logs",
    "schemaName": "serilog",
    "needAutoCreateTable": true,
    "needAutoCreateSchema": true,
    "columnOptionsSection": {
      "message": "RenderedMessage",
      "message_template": "MessageTemplate",
      "level": "Level",
      "raise_date": "Timestamp",
      "exception": "Exception",
      "properties": "LogEventSerialized",
      "user_name": {
        "Name": "SingleProperty",
        "Args": { "propertyName": "UserName" }
      },
      "client_ip": {
        "Name": "SingleProperty",
        "Args": { "propertyName": "ClientIp" }
      },
      "source_context": {
        "Name": "SingleProperty",
        "Args": { "propertyName": "SourceContext" }
      }
    }
  }
}
```

| Setting | Description |
|---------|-------------|
| `connectionString` | Connection string name from ConnectionStrings section |
| `tableName` | Database table name for logs |
| `schemaName` | Database schema name |
| `needAutoCreateTable` | Auto-create table if not exists |
| `needAutoCreateSchema` | Auto-create schema if not exists |
| `columnOptionsSection` | Column mapping configuration |

### Console Sink

```json
{
  "Name": "Console",
  "Args": {
    "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console",
    "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {ClientIp} {UserName} <s:{SourceContext}>{NewLine}{Exception}"
  }
}
```

## Other Settings

```json
{
  "DetailedErrors": true,
  "AllowedHosts": "*"
}
```

| Setting | Default | Description |
|---------|---------|-------------|
| `DetailedErrors` | false | Show detailed error messages (development only) |
| `AllowedHosts` | * | Allowed host headers (* for all) |

## Example Complete Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=cv4pve_admin;Username=postgres;Password=secret"
  },
  "CookieSettings": {
    "ExpireDays": 30
  },
  "Security": {
    "PasswordOptions": {
      "RequiredLength": 12,
      "RequireNonAlphanumeric": true,
      "RequireDigit": true,
      "RequireLowercase": true,
      "RequireUppercase": true
    },
    "LockoutOptions": {
      "MaxFailedAccessAttempts": 3,
      "AllowedForNewUsers": true,
      "DefaultLockoutTimeSpan": "00:30:00"
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "./data/logs/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  },
  "AllowedHosts": "*"
}
```
