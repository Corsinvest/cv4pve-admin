/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Hooks;

public enum WebHookAuthType
{
    None = 0,
    Basic = 1,
    Bearer = 2,
    ApiKey = 3
}
