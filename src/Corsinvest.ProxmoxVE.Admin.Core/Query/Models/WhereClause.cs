/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Models;

/// <summary>
/// Represents a WHERE clause, or a nested group, with conditions and logic operator
/// </summary>
public class WhereClause : WhereNode
{
    public LogicOperator Logic { get; set; } = LogicOperator.And;
    public List<WhereNode> Conditions { get; set; } = [];

    public override WhereClause Clone()
        => new()
        {
            Logic = Logic,
            Conditions = [.. Conditions.Select(c => c.Clone())]
        };
}
