# Notification Hub

Configure notification channels to receive alerts from cv4pve-admin modules (job completion, errors, scheduled reports).

<div class="grid cards" markdown>

-   :material-email:{ .lg .middle } **SMTP** <span class="ce"></span>

    ---

    Send notifications via email using any SMTP server.

    [:octicons-arrow-right-24: Configure SMTP](#smtp)

-   :material-webhook:{ .lg .middle } **WebHook** <span class="ce"></span>

    ---

    Send HTTP requests to any URL with customizable method, body and authentication.

    [:octicons-arrow-right-24: Configure WebHook](#webhook)

-   :material-bell-plus:{ .lg .middle } **119+ Channels** <span class="ee"></span>

    ---

    Telegram, Discord, Slack, Microsoft Teams, Apprise and many more notification services.

</div>

---

## SMTP

Send notifications via email using any SMTP server.

### Configuration

| Field | Description |
|---|---|
| **Host** | SMTP server hostname or IP |
| **Port** | SMTP port (typically `25`, `465` or `587`) |
| **From Address** | Sender email address |
| **Username** | SMTP authentication username (if required) |
| **Password** | SMTP authentication password (if required) |
| **Enable SSL** | Use SSL/TLS for the connection |
| **To Addresses** | Recipient addresses, separated by `,` `;` or `+` |

---

## WebHook

Send an HTTP request to any URL when a notification is triggered.

### Configuration

| Field | Description |
|---|---|
| **URL** | Endpoint to call |
| **Method** | HTTP method: `GET`, `POST`, `PUT`, `PATCH`, `DELETE` |
| **Body Type** | Format: `JSON`, `XML`, `Text`, or `None` |
| **Body** | Request body. Use `%subject%` and `%body%` as placeholders. Leave empty for default JSON payload. |
| **Auth Type** | `None`, `Basic`, `Bearer`, `ApiKey` |
| **Timeout** | Request timeout in seconds (1–300) |
| **Ignore SSL Certificate** | Skip SSL validation (useful for self-signed certificates) |

### Placeholders

Placeholders are replaced at runtime in the URL, headers and body:

| Placeholder | Description |
|---|---|
| `%subject%` | Notification subject / title |
| `%body%` | Notification message content |

### Default payload

If **Body** is left empty, the following JSON is sent automatically:

```json
{"subject": "%subject%", "body": "%body%"}
```

---

## WebHook Examples

=== "Telegram"

    Send messages to a Telegram chat via Bot API.

    **Prerequisites:**

    1. Open Telegram and search for **@BotFather**
    2. Send `/newbot` and follow the instructions to get your **bot token**
    3. Send any message to your bot, then open in a browser:
       ```
       https://api.telegram.org/bot<TOKEN>/getUpdates
       ```
    4. Find `"chat":{"id": 123456789}` — that number is your **chat_id**

    **Configuration:**

    | Field | Value |
    |---|---|
    | URL | `https://api.telegram.org/bot<YOUR_BOT_TOKEN>/sendMessage` |
    | Method | `POST` |
    | Body Type | `JSON` |
    | Auth | `None` |

    **Body:**

    ```json
    {
      "chat_id": "YOUR_CHAT_ID",
      "text": "<b>%subject%</b>\n\n%body%",
      "parse_mode": "HTML"
    }
    ```

    !!! tip
        Use `parse_mode: "HTML"` to render `<b>`, `<i>` and `<code>` tags in the message.

=== "Microsoft Teams"

    Send messages to a Teams channel via Incoming Webhook.

    **Prerequisites:**

    1. In Teams open the channel → **...** → **Connectors**
    2. Add **Incoming Webhook**, set a name and copy the generated URL

    !!! note
        Microsoft is migrating Teams connectors to **Power Automate workflows**. If the classic connector is unavailable, create a workflow in Teams with an HTTP trigger and use that URL instead.

    **Configuration:**

    | Field | Value |
    |---|---|
    | URL | `https://outlook.office.com/webhook/<YOUR_WEBHOOK_URL>` |
    | Method | `POST` |
    | Body Type | `JSON` |
    | Auth | `None` |

    **Body:**

    ```json
    {
      "@type": "MessageCard",
      "@context": "http://schema.org/extensions",
      "summary": "%subject%",
      "themeColor": "0076D7",
      "sections": [
        {
          "activityTitle": "%subject%",
          "activityText": "%body%"
        }
      ]
    }
    ```

=== "Slack"

    Send messages to a Slack channel via Incoming Webhook.

    **Prerequisites:**

    1. Go to [api.slack.com/apps](https://api.slack.com/apps) → **Create App** → **Incoming Webhooks**
    2. Activate and copy the Webhook URL

    **Configuration:**

    | Field | Value |
    |---|---|
    | URL | `https://hooks.slack.com/services/YOUR/WEBHOOK/URL` |
    | Method | `POST` |
    | Body Type | `JSON` |
    | Auth | `None` |

    **Body:**

    ```json
    {
      "text": "*%subject%*\n%body%"
    }
    ```
