# Changelog

All notable changes to cv4pve-admin will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

## [2.0.0-rc6] - 2026-03-04

### Community Edition

#### Added

- **AI Server / MCP** — refactored MCP infrastructure with ToolHelper, McpBridge, ApiAccess/Bridge tabs
- **AI Server / MCP** — upgrade McpBridge to official MCP SDK 1.0.0; log MCP requests to audit log
- **Automatic API token wizard** — guided creation of API tokens during cluster registration
- **PvePermissions model** — cluster permission caching
- **Health Score** — for nodes, VMs, and storages; added to Resources column picker and widget icons
- **Sparklines** — CPU/RAM history sparklines in VM and Node summary components
- **Cluster name in URL** — multi-tab isolation and shareable deep links (`/module/{cluster}/{slug}`)
- **Query param deep-linking** — URL navigation in Resources, VM/Node managers, and search system
- **Localization system** — JSON-based localization with culture support
- **Culture preference** — language/culture selection in user profile
- **CopyValueDialog / CredentialDialog** — shared copy and credential display components
- **OpenCopyValueAsync** — dialog helper for token/value display
- **Resizable side dialog** — drag-to-resize side panel; EnumDropDown component
- **SshRequiredGate** — gate component in NodeProtect/Updater for SSH requirement enforcement
- **Disable console/SSH** for non-PAM or API Token users
- **Default password notification** — alert when default admin password is still in use
- **Icons** — added to Diagnostic IssuesDialog fields and HealthScore widget
- **Dynamic Documentation link** — help menu link adapts to current module
- **"Who is using"** — HelpMenu entry with cluster info pre-fill

#### Changed

- Move ClusterSelector to Layout namespace; add UrlHelper
- Move SettingSection/SettingsAccordion from Module.System to Core
- Standardize audit logging format
- Unify Job/ActionHelper scan signature across analytics modules
- Core misc improvements — BuildInfo.IsInContainer, SyncRolesAsync, EditDialog cleanup
- Extract default admin credentials into ApplicationHelper constants
- Update default dashboard configuration
- Simplify AI Server VmTools; add tags to minimal VM output
- Remove redundant title/aria-label from buttons with visible Text
- Upgrade Radzen to 9.0.8

#### Fixed

- Prevent concurrent RefreshDataAsync calls with SemaphoreSlim
- Expand resource rows after data load instead of OnAfterRenderAsync
- PasswordField Start/End slots reorder; use Variant.Text for toggle
- Various minor UI and logic fixes

### Enterprise Edition

#### Added

- **DRS** — unified Dashboard with Resource Distribution (CPU/RAM barchart + RRD sparklines), HA Groups support, full run history with migrations detail
- **AI Server** — new MCP tools: `ListBackups`, `ListStorageContent`, `ListTasks`; PSI Pressure metrics in RRD data
- **AppTokens** — authentication with bulk permission operations; AppTokenPermissionsGrid, PermissionsSummaryDialog
- **PermissionsEditor** — AppTokens UI, built-in role protection, PermissionsSummaryDialog service
- **AIServer EE permissions** — extended MCP tool permissions model

#### Changed

- Audit log — copy button in audit log details
- Adapt EE to URL-based cluster routing (cluster name in URL)

## [2.0.0-rc5] - 2026-02-28

### Added

- **AI Server** — McpBridge build support; PVEAdmin role docs and AI Server output formats
- **Connecting AI Clients** — MCP Bridge section in AI Server documentation

### Changed

- Update NuGet packages

### Fixed

- CSS assets Docker build alignment

## [2.0.0-rc4] - 2026-02-19

### Fixed

- CSS assets returning 500 error in production (Release build)
- Docker build aligned to production configuration

## [2.0.0-rc3] - 2026-02-19

### Added

- **RangeSelector component** — date/time range selection with drag handles
- **"Add cluster" button** — pulsing button in header when no clusters are configured
- **Memo widget** — content editing moved to settings panel
- **Subscription page** *(EE)* — alert shown when running in testing/pre-release mode

### Changed

- Dashboard widget title bar hidden when title is empty
- Dashboard clone now copies widget size
- ResourceUsageGaugeStacked default size adjusted

### Fixed

- Cluster deletion now refreshes header and navigation immediately
- Various Docker startup and CSS asset fixes

## [2.0.0-rc2] - 2026-02-18

### Added

- **WebHook notifier module** — send notifications to any HTTP endpoint on Proxmox VE events
- **MarkdownEditor component** — Write/Preview tabs for markdown fields across the UI
- **ResourceUsageGaugeStacked widget** — concentric arc gauges for CPU, memory and storage usage
- **Help menu** — separate items for Report a Bug, Request a Feature and Provide Feedback, with bug report pre-filled with version, cluster, Proxmox VE version, browser and platform
- **Help menu** — red dot badge on help icon and "Update available" notification when a new release is available
- **Icons** added to form fields across the UI for better visual clarity
- **HTTPS reverse proxy guide** in documentation

### Changed

- AutoSnap webhooks refactored to use ExtendedData for cleaner configuration
- Dashboard reset logic improved, save icon added
- Send test email button placed inline with the email field
- Telegram bot info dialog replaced with link to documentation

### Fixed

- Installer interactive prompts fixed
- `appsettings.extra.json` pre-created on Windows to prevent Docker creating it as a directory

## [2.0.0-rc1] - 2026-02-12

### Initial Release

This is the first public release of **cv4pve-admin v2** — a complete rewrite of the original project built on .NET 9 and Blazor.

Starting from this release, all notable changes will be tracked in this file.

#### Modules (Community Edition)

- **AutoSnap** — Automated snapshot scheduling with hooks, history, and timeline view
- **Backup Analytics** — Backup trend analysis with size, speed, and duration charts
- **Replication Analytics** — Replication job monitoring and failure tracking
- **Diagnostic** — Cluster health diagnostics with issue scanning and PDF export
- **Dashboard** — Customizable widget-based dashboard with multi-cluster support
- **Node Protect** — Node configuration backup with pluggable storage providers
- **Metrics Exporter** — Export cluster metrics to external monitoring systems
- **Notifier** — Notification system with SMTP support
- **AI Server** — Model Context Protocol (MCP) server for AI assistant integration
- **Bots** — Telegram bot integration for remote cluster management
- **UPS Monitor** — UPS status monitoring via NUT protocol
- **System Report** — System information and configuration report generation
- **Updater** — Application self-update management
- **Resources** — Cluster resource usage overview

#### Modules (Enterprise Edition)

- **Portal** — Self-service portal for VM management by end users
- **Workflow** — Visual workflow automation for Proxmox VE operations (powered by Elsa)
- **VM Performance** — Advanced VM performance analytics and trending
- **Diagnostic Enterprise** — Extended diagnostics with enterprise checks
- **Node Protect Enterprise** — Extended node protection with additional providers
- **Notifier Enterprise** — Extended notification channels (Apprise, webhooks, etc.)
- **AI Server Enterprise** — Extended MCP tools for enterprise operations
- **System Enterprise** — Extended system management and configuration
- **Profile Enterprise** — Extended user profile and SSO integration

#### Core Features

- Multi-cluster management from a single interface
- Command Palette for quick navigation and actions
- Role-based access control with granular module permissions
- Audit logging for security-relevant events
- Dark/light theme with Fluent design
- Keyboard shortcuts
- Help menu with release notes, documentation links, and feedback
- Docker, native, and .NET Aspire (AppHost) deployment support
