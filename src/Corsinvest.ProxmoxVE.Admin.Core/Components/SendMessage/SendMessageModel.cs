/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Session;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.SendMessage;

public class SendMessageModel
{
    [Required] public string Title { get; set; } = default!;
    [Required] public string Message { get; set; } = default!;
    public MessageSeverity Severity { get; set; } = MessageSeverity.Info;
}
