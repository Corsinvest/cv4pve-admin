/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.RegularExpressions;

namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

/// <summary>
/// Helpers for safely building PostgreSQL raw SQL statements.
/// </summary>
public static partial class PostgreSqlHelper
{
    private const int MaxIdentifierLength = 63; // PostgreSQL NAMEDATALEN - 1

    [GeneratedRegex(@"^[A-Za-z_][A-Za-z0-9_]*$")]
    private static partial Regex IdentifierRegex();

    /// <summary>
    /// Validates a PostgreSQL identifier (database, schema, table, column) before interpolation
    /// into raw SQL. Accepts only <c>[A-Za-z_][A-Za-z0-9_]*</c> up to 63 chars.
    /// Throws <see cref="InvalidOperationException"/> if the identifier contains characters
    /// that could allow SQL injection.
    /// </summary>
    public static string SafeIdentifier(string? identifier)
    {
        if (string.IsNullOrEmpty(identifier)
            || identifier.Length > MaxIdentifierLength
            || !IdentifierRegex().IsMatch(identifier))
        {
            throw new InvalidOperationException($"Invalid PostgreSQL identifier: '{identifier}'");
        }
        return identifier;
    }

    /// <summary>
    /// Returns a double-quoted, safe PostgreSQL identifier ready for inclusion in raw SQL
    /// (e.g. <c>"mydb"</c>). The inner value is validated with <see cref="SafeIdentifier"/>.
    /// </summary>
    public static string QuoteIdentifier(string? identifier)
        => $"\"{SafeIdentifier(identifier)}\"";
}
