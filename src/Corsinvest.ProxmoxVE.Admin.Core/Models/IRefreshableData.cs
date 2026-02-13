/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public interface IRefreshableData
{
    static IRefreshableData Dummy = new DummyRefreshable();

    Task RefreshDataAsync();
}
