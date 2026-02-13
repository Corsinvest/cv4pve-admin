/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Executor;

/// <summary>
/// Interface for providing queryable data sources
/// </summary>
public interface IDataProvider
{
    Task<IQueryable> GetAsync(string tableName);
}
