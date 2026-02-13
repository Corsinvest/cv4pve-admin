/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using MimeKit;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface IEmailSender
{
    // TODO: Refactor to avoid MimeKit dependency in interface signature
    // Consider using simple parameters: Task SendEmailAsync(string to, string subject, string body, SmtpEmailConfig config, bool isHtml = false)
    Task SendEmailAsync(MimeMessage message, SmtpEmailConfig smtpEmailConfig);
}
