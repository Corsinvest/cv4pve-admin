/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Models;

/// <summary>
/// Represents the result of a query execution
/// </summary>
/// <param name="Query">Original query that was executed</param>
/// <param name="Results">Result data as dynamic list</param>
public record QueryResult(Query Query, List<dynamic> Results);
