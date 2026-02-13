# Changelog

All notable changes to cv4pve-admin will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

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
- **DDR** — Disaster Recovery with automated failover and replication management
- **DRS** — Dynamic Resource Scheduling for automatic VM balancing
- **Workflow** — Visual workflow automation for Proxmox VE operations (powered by Elsa)
- **AI** — Artificial Intelligence assistant for cluster management
- **VM Performance** — Advanced VM performance analytics and trending
- **Diagnostic Enterprise** — Extended diagnostics with enterprise checks
- **Node Protect Enterprise** — Extended node protection with additional providers
- **Notifier Enterprise** — Extended notification channels (Apprise, webhooks, etc.)
- **AI Server Enterprise** — Extended MCP tools for enterprise operations
- **System Enterprise** — Extended system management and configuration
- **Updater Enterprise** — Enterprise update management with rollback support
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
