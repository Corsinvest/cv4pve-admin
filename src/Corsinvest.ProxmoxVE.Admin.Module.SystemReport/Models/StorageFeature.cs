/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Models;

[Flags]
public enum StorageFeature
{
    All = 0,
    Content = 1,
    RrdData = 2
}
