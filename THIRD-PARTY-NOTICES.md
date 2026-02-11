# Third-Party Notices and Legal Disclaimers

This project incorporates components from the following third-party projects. The original copyright statements and license terms are linked below.

---

## 1. Core Frameworks & Runtimes

| Component | License | Project / Source |
| --- | --- | --- |
| **.NET Runtime & SDK** | MIT | [dotnet/runtime](https://github.com/dotnet/runtime) |
| **ASP.NET Core Framework** | MIT | [dotnet/aspnetcore](https://github.com/dotnet/aspnetcore) |
| **.NET Aspire SDK** | MIT | [dotnet/aspire](https://github.com/dotnet/aspire) |

---

## 2. .NET NuGet Packages

| Package / Group | License | Project Link |
| --- | --- | --- |
| **Radzen.Blazor** | MIT | [Radzen.Blazor](https://github.com/radzenhq/radzen-blazor) |
| **Entity Framework Core** | MIT | [EF Core](https://github.com/dotnet/efcore) |
| **Npgsql (EF Core PostgreSQL)** | PostgreSQL | [Npgsql](https://github.com/npgsql/efcore.pg) |
| **Serilog Ecosystem** | Apache-2.0 | [Serilog](https://github.com/serilog/serilog) |
| **OpenTelemetry .NET** | Apache-2.0 | [OpenTelemetry](https://github.com/open-telemetry/opentelemetry-dotnet) |
| **Hangfire (Core & Storage)** | LGPL-3.0 | [Hangfire](https://github.com/HangfireIO/Hangfire) |
| **ClosedXML & Reports** | MIT | [ClosedXML](https://github.com/ClosedXML/ClosedXML) |
| **PDFsharp & MigraDoc** | MIT | [PDFsharp](https://github.com/empira/PDFsharp) |
| **Blazored (Storage & Session)** | MIT | [Blazored](https://github.com/Blazored) |
| **FluentResults** | MIT | [FluentResults](https://github.com/altmann/FluentResults) |
| **Humanizer** | MIT | [Humanizer](https://github.com/Humanizr/Humanizer) |
| **MailKit** | MIT | [MailKit](https://github.com/jstedfast/MailKit) |
| **Mapster** | MIT | [Mapster](https://github.com/MapsterMapper/Mapster) |
| **Model Context Protocol (MCP)** | Apache-2.0 | [MCP SDK](https://github.com/modelcontextprotocol/csharp-sdk) |

### Proxmox VE Integration (Corsinvest)

*Licensed under **GPL-3.0-only**:*
[cv4pve-api-dotnet](https://github.com/Corsinvest/cv4pve-api-dotnet) | [cv4pve-autosnap](https://github.com/Corsinvest/cv4pve-autosnap) | [cv4pve-diag](https://github.com/Corsinvest/cv4pve-diag) | [cv4pve-metrics-exporter](https://github.com/Corsinvest/cv4pve-metrics-exporter) | [cv4pve-botgram](https://github.com/Corsinvest/cv4pve-botgram)

---

## 3. Docker Container Images

| Component | License | Source / Repository |
| --- | --- | --- |
| **PostgreSQL Database** | PostgreSQL | [PostgreSQL (Official Image)](https://hub.docker.com/_/postgres) |
| **PgWeb (DB Admin)** | MIT | [sosedoff/pgweb](https://github.com/sosedoff/pgweb) |
| **Watchtower (Auto Updates)** | Apache-2.0 | [containrrr/watchtower](https://github.com/containrrr/watchtower) |
| **Apprise (Notifications)** | MIT | [caronc/apprise-api](https://github.com/caronc/apprise-api) |

---

## 4. License Compatibility

CV4PVE Admin Community Edition is licensed under **AGPL-3.0-only**.

* ✅ **Permissive (MIT, Apache-2.0, PostgreSQL):** Fully compatible.
* ✅ **Weak Copyleft (LGPL-3.0):** Compatible via dynamic linking as used.
* ⚠️ **Strong Copyleft (GPL-3.0-only):** Compatible when used in accordance with Corsinvest’s specific architectural requirements.

---

## 5. Additional Information

* **Transitive Dependencies:** All sub-dependencies are governed by OSI-approved licenses (primarily MIT or Apache-2.0).
* **Note on Docker Images:** These images are pulled at runtime during deployment and are not bundled with the source code.
* **Enterprise Edition:** Proprietary modules are subject to a separate commercial license agreement.

**Contact:** Corsinvest Srl | support@corsinvest.it | [www.corsinvest.it](https://www.corsinvest.it)

---

*Document updated: February 2026*
