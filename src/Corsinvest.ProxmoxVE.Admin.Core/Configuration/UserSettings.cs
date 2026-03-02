/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Configuration;

public class UserSettings
{
    public string Culture { get; set; } = ApplicationHelper.DefaultCulture;
}
