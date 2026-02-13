/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public class ApplicationStateService
{
    public bool IsStartupComplete { get; set; }
    public bool IsReady { get; set; }
}
