/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json.Serialization;

namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Models;

/// <summary>
/// Represents an element of a WHERE clause: a single condition or a group of nested conditions
/// </summary>
[JsonConverter(typeof(WhereNodeJsonConverter))]
public abstract class WhereNode
{
    public abstract WhereNode Clone();
}
