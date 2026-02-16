/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.Notifier.WebHook;

public class Module : NotifierModuleBase<Settings, Components.Render>
{
    public Module() : base("WebHook")
    {
        Icon = "webhook";
    }
}
