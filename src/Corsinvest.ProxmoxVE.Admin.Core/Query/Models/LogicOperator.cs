/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json.Serialization;

namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Models;

/// <summary>
/// Logic operator joining the conditions of a WHERE clause
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<LogicOperator>))]
public enum LogicOperator
{
    And,
    Or
}
