---
draft: true
---

# <span class="ee"></span> :material-auto-awesome: AI Assistant <span class="scope" data-scope="all-clusters"></span>

!!! warning "Coming soon"
    AI Assistant is not yet released. This page is a preview of the planned feature and may change before the module ships.

Built-in chat assistant integrated directly into cv4pve-admin — accessible from the main header without leaving the application.

## Features

<div class="grid cards" markdown>

- :material-chat:{ .lg .middle } **In-app Chat**

    ---

    Chat panel reachable from the top header — ask questions about the cluster without context-switching to a separate tool.

- :material-cloud:{ .lg .middle } **Hosted API**

    ---

    Talks to a Corsinvest-hosted AI endpoint by default (`cv4pve-admin.corsinvest.it/api`) — bring your own key.

- :material-format-list-bulleted:{ .lg .middle } **Multi-format Output**

    ---

    Responses can be rendered as HTML or Markdown with optional value decoration (emojis on thresholds for at-a-glance status).

- :material-table:{ .lg .middle } **CSV Export**

    ---

    Tabular replies can be exported as CSV with a configurable separator.

</div>

## Why

Why an in-app assistant when AI Server already exposes MCP?

<div class="why-grid" markdown>

<div markdown>
!!! tip "No external client"
    For occasional questions, no MCP client or external chat app is needed — open the panel, type, read.
</div>

<div markdown>
!!! success "Same session, same context"
    The assistant lives inside the application the user already has open — answers reference the cluster you are currently on.
</div>

<div markdown>
!!! info "Complement to [AI Server](ai-server.md)"
    AI Server is for external assistants (Claude Desktop, Cursor, …) over MCP. AI Assistant is for in-app, casual questions.
</div>

</div>

## Settings

??? note settings "Show all settings"

    **General**

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **Enabled** | off | Master on/off switch for the assistant |
    | **Api Key** | – | Key used to authenticate against the AI API |
    | **Api Service Url** | `https://cv4pve-admin.corsinvest.it/api` | Endpoint of the AI service backing the assistant |

    **CSV**

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **Separator** | `;` | Column separator used when exporting tabular replies as CSV |

    **HTML**

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **Decorate Value With Emoji** | on | Add emoji indicators based on warning/critical thresholds |
    | **Threshold → Warning** | 80 | Value at or above which the warning indicator is used |
    | **Threshold → Critical** | 90 | Value at or above which the critical indicator is used |

    **Markdown**

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **Decorate Value With Emoji** | on | Add emoji indicators based on warning/critical thresholds |
    | **Threshold → Warning** | 80 | Value at or above which the warning indicator is used |
    | **Threshold → Critical** | 90 | Value at or above which the critical indicator is used |
