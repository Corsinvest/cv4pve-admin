# Bots

Remote cluster management via Telegram chatbot.

## Overview

Remote Proxmox VE cluster management through Telegram chatbot. Execute commands, monitor status, and manage operations via Telegram.

<div class="grid cards" markdown>

-   :material-remote:{ .lg .middle } **Remote Control**

    ---

    Execute commands and manage nodes through Telegram bot.

-   :material-bell-alert:{ .lg .middle } **Notifications**

    ---

    Receive notifications for cluster events and status changes.

-   :material-lock:{ .lg .middle } **Authentication**

    ---

    Token-based authentication for bot access control.

-   :material-cellphone:{ .lg .middle } **Mobile Access**

    ---

    Manage cluster from any device with Telegram app.

-   :material-flash:{ .lg .middle } **Command Response**

    ---

    Receive feedback on commands and system status.

-   :material-message-text:{ .lg .middle } **Chat Interface**

    ---

    Execute operations through chat-based commands.

</div>

## Set Up Your Telegram Bot

To connect the Bots module to Telegram, you need to create a bot via **BotFather** and copy the generated token into the module settings.

1. Open the Telegram app on your phone or computer.

2. Search for the [**BotFather**](https://web.telegram.org/k/#@BotFather) bot — he's the official Telegram bot for creating and managing bots.

3. Type `/newbot` to create a new bot. Follow the instructions:
    - Choose a **display name** (e.g. `My PVE Bot`)
    - Choose a **username** — must be unique and end in `bot` (e.g. `my_pve_bot`)

4. BotFather will generate an **API token** (e.g. `707587383:AAHD9D***************`).

5. Copy the token and paste it into the **Token** field in the Bots module settings.

!!! tip
    If you are creating a bot just for testing, prefix the username with your name to make it unique (e.g. `john_pve_bot`).
