# :material-cog: General Settings

Global application configuration. Settings live in the database (not in `appsettings.json`) so they survive container updates.

## Sections

- **Application** — app name, default theme, default language
- <span class="ce"></span> **SMTP** — outbound email server
- <span class="ee"></span> **Appearance** — logo, colours, login page customisation
- **Release Channel** — stable / pre-release for the Updater module

## Application

| Field | Description |
|-------|-------------|
| **App Name** | Shown in the browser tab title and the top header. |
| **Default theme** | Light or dark — applied to new users and to the login page. Each user can still override it from their [Profile](../profile.md). |
| **Default language** | Locale used for new users. Each user can override it from their Profile. |

## <span class="ce"></span> SMTP

| Field | Description |
|-------|-------------|
| **Host / Port** | SMTP server address and port. |
| **Encryption** | `None`, `SSL` or `STARTTLS`. |
| **Username / Password** | Credentials (optional if your relay accepts anonymous send from the cv4pve-admin host). |
| **Default sender** | `From:` address used by transactional emails (activation, password reset) and by the SMTP notifier. |

The **Send test email** button verifies the configuration end-to-end.

The same SMTP server is reused by the SMTP notifier — see [Notification Hub](notifier.md).

## <span class="ee"></span> Appearance

Customise how the UI looks to your users.

| Field | Description |
|-------|-------------|
| **Logo** | Custom logo image shown in the top header and on the login page. |
| **Favicon** | Browser tab icon. |
| **Primary colour** | Overrides the default indigo theme accent. |
| **Login page** | Background image, welcome text, footer text. |

Useful for MSPs and enterprises that want to brand the admin interface for their own customers.

## Release Channel

Controls which release stream the [Updater](../../modules/update-manager.md) and the in-app "new version available" indicator look at.

| Channel | What it shows |
|---------|---------------|
| **Stable** | Only final releases (e.g. `2.1.0`). Recommended for production. |
| **Pre-release** | Includes `-rc`, `-beta`, `-alpha` builds. Useful to evaluate upcoming features. |
| **All** | Shows everything — typically used by maintainers / testers. |

