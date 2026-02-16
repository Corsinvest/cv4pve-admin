/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.Notifier.Smtp;

public class Module : NotifierModuleBase<Settings, Components.Render>
{
    public Module() : base("SMTP")
    {
        Icon = "email";
    }
}
