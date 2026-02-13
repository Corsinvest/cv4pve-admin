/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Models;

/// <summary>
/// Represents a WHERE clause with conditions and logic operator
/// </summary>
public class WhereClause
{
    public string Logic { get; set; } = "and";
    public List<Condition> Conditions { get; set; } = [];

    public WhereClause Clone()
        => new()
        {
            Logic = Logic,
            Conditions = [.. Conditions.Select(c => c.Clone())]
        };
}
